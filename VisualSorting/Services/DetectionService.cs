using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HalconDotNet;
using VisualSorting.Models;

namespace VisualSorting.Services
{
    /// <summary>
    /// 视觉检测服务实现类
    /// 核心算法类：执行模板匹配和芯片针脚检测
    /// 检测流程：模板定位 -> 仿射变换 -> 针脚计数 -> 合格判定
    /// 
    /// 触发逻辑说明：
    /// - 基于模板匹配结果触发，不再使用基准线检测
    /// - 第一次识别到芯片（模板匹配成功）时触发上升沿
    /// - 2秒后自动触发下降沿，重置状态
    /// - 继续等待下一次识别
    /// </summary>
    public class DetectionService : IDetectionService
    {
        #region 私有字段
        /// <summary>
        /// 模板服务依赖
        /// 用于获取模板ID、轮廓和模板区域
        /// </summary>
        private readonly ITemplateService _templateService;

        /// <summary>
        /// 形状模板ID（克隆自TemplateService）
        /// Halcon内部句柄，用于模板匹配
        /// </summary>
        private HTuple hv_ModelID;

        /// <summary>
        /// 模板轮廓（克隆自TemplateService）
        /// 用于显示匹配位置
        /// </summary>
        private HObject ho_ModelContours;

        /// <summary>
        /// 检测区域1（芯片针脚区域）
        /// XLD轮廓格式，需要仿射变换后使用
        /// </summary>
        private HObject ho_checkRectXid;

        /// <summary>
        /// 检测区域2（芯片针脚区域）
        /// XLD轮廓格式，需要仿射变换后使用
        /// </summary>
        private HObject ho_checkRectXid2;

        /// <summary>
        /// 变换后的检测区域1
        /// 经过仿射变换对齐到当前芯片位置
        /// </summary>
        private HObject ho_r2Rectangle;

        /// <summary>
        /// 变换后的检测区域2
        /// 经过仿射变换对齐到当前芯片位置
        /// </summary>
        private HObject ho_r3Rectangle;

        /// <summary>
        /// 取消令牌源（用于控制检测循环）
        /// 当前未使用，预留扩展
        /// </summary>
        private CancellationTokenSource _captureCts;

        /// <summary>
        /// 是否已触发（防同一芯片重复触发）
        /// </summary>
        private bool _hasTriggered = false;

        /// <summary>
        /// 上升沿触发时间戳，用于计时判定下降沿
        /// </summary>
        private DateTime _risingEdgeTime = DateTime.MinValue;

        /// <summary>
        /// 下降沿超时时间（毫秒），上升沿触发后经过此时间自动触发下降沿
        /// </summary>
        private const int FALLING_EDGE_TIMEOUT_MS = 2500;

        /// <summary>
        /// 检测区域1针脚数合格下限，默认4
        /// </summary>
        private int _pinCountMin = 4;

        /// <summary>
        /// 检测区域1针脚数合格上限，默认4
        /// </summary>
        private int _pinCountMax = 4;

        /// <summary>
        /// 检测区域2针脚数合格下限，默认4
        /// </summary>
        private int _pinCount2Min = 4;

        /// <summary>
        /// 检测区域2针脚数合格上限，默认4
        /// </summary>
        private int _pinCount2Max = 4;
        #endregion

        #region 属性
        /// <summary>
        /// 检测区域1是否已创建
        /// </summary>
        public bool IsCheckRectXidCreated => ho_checkRectXid != null && ho_checkRectXid.IsInitialized();

        /// <summary>
        /// 检测区域2是否已创建
        /// </summary>
        public bool IsCheckRectXid2Created => ho_checkRectXid2 != null && ho_checkRectXid2.IsInitialized();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数：注入模板服务依赖
        /// </summary>
        public DetectionService(ITemplateService templateService)
        {
            _templateService = templateService;
            if (templateService == null)
            {
                throw new ArgumentNullException(nameof(templateService), "模板服务不能为空");
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 设置模板和检测区域
        /// 在启动检测前必须调用此方法
        /// </summary>
        public void SetTemplate(HTuple modelID, HObject modelContours, HObject checkRectXid, HObject checkRectXid2)
        {
            // 参数验证：确保所有输入参数有效
            if (modelID == null || modelID.Length == 0)
            {
                throw new ArgumentNullException(nameof(modelID), "模板ID无效");
            }
            if (modelContours == null || !modelContours.IsInitialized())
            {
                throw new ArgumentNullException(nameof(modelContours), "模板轮廓无效");
            }
            if (checkRectXid == null || !checkRectXid.IsInitialized())
            {
                throw new ArgumentNullException(nameof(checkRectXid), "检测区域1无效");
            }
            if (checkRectXid2 == null || !checkRectXid2.IsInitialized())
            {
                throw new ArgumentNullException(nameof(checkRectXid2), "检测区域2无效");
            }
            if (!_templateService.IsTemplateCreated)
            {
                throw new Exception("模板未创建，请先绘制模板");
            }

            // 释放旧资源，避免内存泄漏
            hv_ModelID?.Dispose(); // ?.空条件运算符，如果对象为null则不执行Dispose
            ho_ModelContours?.Dispose();
            ho_r2Rectangle?.Dispose();
            ho_r3Rectangle?.Dispose();

            // 克隆新资源：Clone()创建独立副本，避免外部修改影响内部数据
            hv_ModelID = modelID.Clone();
            ho_ModelContours = modelContours.Clone();
            ho_r2Rectangle = checkRectXid.Clone();
            ho_r3Rectangle = checkRectXid2.Clone();
        }

        /// <summary>
        /// 重置触发状态
        /// 手动重置触发状态，准备下一次触发
        /// </summary>
        public void ResetTriggerState()
        {
            _hasTriggered = false;
            _risingEdgeTime = DateTime.MinValue;
        }

        /// <summary>
        /// 设置针脚数合格范围
        /// 默认要求两个检测区域各恰好4个针脚才算合格
        /// 可通过此方法放宽判定条件，例如设为3-5允许一定误差
        /// </summary>
        public void SetPinCountRange(int pinCountMin, int pinCountMax, int pinCount2Min, int pinCount2Max)
        {
            _pinCountMin = pinCountMin;
            _pinCountMax = pinCountMax;
            _pinCount2Min = pinCount2Min;
            _pinCount2Max = pinCount2Max;
        }
        #endregion

        #region 核心检测方法
        /// <summary>
        /// 处理输入图像，执行完整的检测流程
        /// 流程：灰度转换 -> 模板匹配 -> 仿射变换 -> 针脚检测 -> 结果判定
        /// 
        /// 触发逻辑：
        /// - 模板匹配成功时，如果未触发过，则触发上升沿
        /// - 上升沿触发2秒后，自动触发下降沿并重置状态
        /// - 下降沿后可以再次触发上升沿
        /// </summary>
        /// <param name="ho_Image">待检测图像</param>
        /// <returns>检测结果对象</returns>
        public DetectionResult Process(HObject ho_Image)
        {
            var result = new DetectionResult();

            // ========== 输入验证 ==========
            if (ho_Image == null || !ho_Image.IsInitialized())
            {
                result.IsError = true;
                result.ErrorMessage = "输入图像无效";
                result.ResultText = "检测出错";
                return result;
            }
            if (!_templateService.IsTemplateCreated)
            {
                result.IsError = true;
                result.ErrorMessage = "模板未创建，请先创建模板";
                result.ResultText = "模板未创建";
                return result;
            }
            if (_templateService.TemplateRegion == null || !_templateService.TemplateRegion.IsInitialized())
            {
                result.IsError = true;
                result.ErrorMessage = "模板区域无效,请重新绘制模板";
                result.ResultText = "模板区域无效";
                return result;
            }

            HTuple hv_ModelID = _templateService.ModelID;
            if (hv_ModelID == null || hv_ModelID.Length == 0)
            {
                result.IsError = true;
                result.ErrorMessage = "模板未设置或无效"; 
                result.ResultText = "模板ID无效";
                return result;
            }

            // ========== 计时判定下降沿 ==========
            // 在每次处理图像时检查是否应该触发下降沿
            // 条件：已触发 && 距离上升沿触发时间超过2秒
            if (_hasTriggered && _risingEdgeTime != DateTime.MinValue)
            {
                TimeSpan elapsed = DateTime.Now - _risingEdgeTime;
                if (elapsed.TotalMilliseconds >= FALLING_EDGE_TIMEOUT_MS)
                {
                    result.IsFallingEdge = true;
                    // 超时自动触发下降沿，重置状态准备下一次触发
                    _hasTriggered = false;
                    _risingEdgeTime = DateTime.MinValue;
                }
            }

            // ========== 临时变量定义 ==========
            // 所有Halcon对象必须先声明为null，使用后必须Dispose
            HObject ho_GrayImage = null;                    // 灰度图像
            HObject ho_ContoursAffineTrans = null;          // 仿射变换后的轮廓
            HObject ho_Region = null;                       // 检测区域1（填充）
            HObject ho_Region2 = null;                      // 检测区域2（填充）
            HObject ho_checkRectXid = null;                 // 变换后的检测区域1轮廓
            HObject ho_checkRectXid2 = null;                // 变换后的检测区域2轮廓
            HObject ho_ImageReducedCheckArea = null;        // 检测区域1的图像
            HObject ho_ImageReducedCheckArea2 = null;       // 检测区域2的图像
            HObject ho_Region3 = null;                      // 检测区域1二值化结果
            HObject ho_Region4 = null;                      // 检测区域2二值化结果
            HObject ho_RegionFillUp = null;                 // 填充后的区域1
            HObject ho_RegionFillUp2 = null;                // 填充后的区域2
            HObject ho_RegionOpening = null;                // 开运算后的区域1
            HObject ho_RegionOpening2 = null;               // 开运算后的区域2
            HObject ho_RegionClosing = null;                // 闭运算后的区域1
            HObject ho_RegionClosing2 = null;               // 闭运算后的区域2
            HObject ho_ConnectedRegions = null;             // 连通域1
            HObject ho_ConnectedRegions2 = null;            // 连通域2
            HObject ho_SelectedRegions = null;              // 筛选后的区域1
            HObject ho_SelectedRegions2 = null;             // 筛选后的区域2
            HTuple hv_fRow = new HTuple();                  // 匹配位置行坐标
            HTuple hv_fColumn = new HTuple();               // 匹配位置列坐标
            HTuple hv_Angle = new HTuple();                 // 匹配角度
            HTuple hv_Score = new HTuple();                 // 匹配得分
            HTuple hv_HomMat2D = new HTuple();              // 模板变换矩阵
            HTuple hv_HomMat2DCheck = new HTuple();         // 检测区域变换矩阵
            HTuple hv_MRow = new HTuple();                  // 模板中心行坐标
            HTuple hv_MColumn = new HTuple();               // 模板中心列坐标
            HTuple hv_MPhi = new HTuple();                  // 模板角度
            HTuple hv_UsedThreshold = new HTuple();         // 自动阈值
            HTuple hv_Area = new HTuple();                  // 区域面积
            HTuple hv_Row = new HTuple();                   // 区域中心行坐标
            HTuple hv_Column = new HTuple();                // 区域中心列坐标
            HTuple hv_Number = new HTuple();                // 区域数量
            HTuple hv_Number2 = new HTuple();               // 区域数量2

            try
            {
                // ========== 步骤1：灰度转换 ==========
                // 模板匹配需要灰度图像，RGB图像需转换
                HOperatorSet.CountChannels(ho_Image, out HTuple channels);
                if (channels.I == 3)
                {
                    // RGB图像：使用Rgb1ToGray转换为灰度
                    // 公式：Gray = 0.299*R + 0.587*G + 0.114*B
                    HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                }
                else
                {
                    // 已经是灰度图像：直接克隆
                    ho_GrayImage = ho_Image.Clone();
                }

                // ========== 步骤 1.5：图像亮度前置安全校验 (防光源衰减/过曝误检) ==========
                // 工业防呆核心：防止光源衰减、频闪或外界眩光过曝导致特征漏提取误判
                HOperatorSet.Intensity(ho_GrayImage, ho_GrayImage, out HTuple hv_Mean, out HTuple hv_Deviation);
                double meanVal = hv_Mean.D;
                result.BrightnessMean = meanVal;
                hv_Mean.Dispose();
                hv_Deviation.Dispose();

                if (meanVal < 35.0 || meanVal > 220.0)
                {
                    result.IsOK = false;
                    result.DefectType = PinDefectType.LightAbnormal;
                    result.DefectDescription = meanVal < 35.0
                        ? $"光源衰减过暗 (亮度均值 {meanVal:F1} < 35.0)"
                        : $"视野过曝眩光 (亮度均值 {meanVal:F1} > 220.0)";
                    result.ResultText = "NG-光照异常";
                    result.ShouldTrigger = true;
                    result.DisplayImage = ho_GrayImage?.Clone();
                    return result;
                }

                // ========== 步骤2：模板匹配 ==========
                // FindShapeModel：在图像中搜索形状模板
                // 参数说明：
                //   ho_GrayImage - 输入图像
                //   hv_ModelID - 模板ID
                //   -360度到360度 - 搜索角度范围（弧度）
                //   0.6 - 最小匹配得分（0-1，越大越严格）
                //   1 - 最多匹配数量
                //   1 - 最大重叠比例
                //   "least_squares" - 亚像素精度方法
                //   0 - 金字塔层数（0表示自动）
                //   0.7 - 贪婪度（0-1，越大越快但可能漏检）
                HOperatorSet.FindShapeModel(ho_GrayImage, hv_ModelID, (new HTuple(-180)).TupleRad(),
                    (new HTuple(180)).TupleRad(), 0.65, 1, 1, "least_squares", 0, 0.7, out hv_fRow,
                    out hv_fColumn, out hv_Angle, out hv_Score);

                // 更新匹配得分（用于显示）
                result.MatchScore = hv_Score.Length > 0 ? (int)hv_Score[0].D : 0;

                // 显示匹配数量（调试用）
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), new HTuple("匹配到数量：") + (new HTuple(hv_Score.TupleLength())),
                        "window", 10, 10, "green", "box", "true");
                }

                // ========== 步骤3：如果匹配成功，执行针脚检测 ==========
                if (hv_Score.Length > 0)
                {
                    // ========== 上升沿检测（识别到芯片）==========
                    // 上升沿条件：模板匹配成功 && 未触发过
                    if (!_hasTriggered)
                    {
                        result.IsRisingEdge = true;        // 上升沿标志
                        result.ShouldTrigger = true;       // 触发检测标志
                        _hasTriggered = true;              // 标记已触发，防止重复触发
                        _risingEdgeTime = DateTime.Now;    // 记录上升沿触发时间
                    }

                    // 显示模板轮廓（调试用）
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_ModelContours, HDevWindowStack.GetActive());
                    }

                    // 生成刚性变换矩阵
                    // VectorAngleToRigid：计算从(0,0,0)到匹配位置的刚性变换
                    // 刚性变换 = 旋转 + 平移，不包含缩放
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, hv_fRow, hv_fColumn, hv_Angle, out hv_HomMat2D);

                    // 对模板轮廓进行仿射变换
                    // AffineTransContourXld：将轮廓从模板坐标系变换到图像坐标系
                    HOperatorSet.AffineTransContourXld(ho_ModelContours, out ho_ContoursAffineTrans, hv_HomMat2D);

                    // 计算模板区域中心
                    // AreaCenter：计算区域的面积和中心坐标
                    HOperatorSet.AreaCenter(_templateService.TemplateRegion, out var hv__, out hv_MRow, out hv_MColumn);

                    // 生成检测区域的变换矩阵
                    // 从模板中心变换到匹配位置
                    HOperatorSet.VectorAngleToRigid(hv_MRow, hv_MColumn, 0, hv_fRow, hv_fColumn, hv_Angle, out hv_HomMat2DCheck);

                    // 对两个检测区域进行仿射变换
                    // 使检测区域对齐到当前芯片位置
                    HOperatorSet.AffineTransContourXld(ho_r2Rectangle, out ho_checkRectXid, hv_HomMat2DCheck);
                    HOperatorSet.AffineTransContourXld(ho_r3Rectangle, out ho_checkRectXid2, hv_HomMat2DCheck);

                    // 将XLD轮廓转换为填充区域
                    // GenRegionContourXld：将轮廓转换为区域
                    // "filled"参数表示填充轮廓内部
                    HOperatorSet.GenRegionContourXld(ho_checkRectXid, out ho_Region, "filled");
                    HOperatorSet.GenRegionContourXld(ho_checkRectXid2, out ho_Region2, "filled");

                    // 锁定检测区域内的图像
                    // ReduceDomain：将图像的定义域限制在指定区域内
                    // 后续操作只在该区域内进行，提高效率
                    HOperatorSet.ReduceDomain(ho_GrayImage, ho_Region, out ho_ImageReducedCheckArea);
                    HOperatorSet.ReduceDomain(ho_GrayImage, ho_Region2, out ho_ImageReducedCheckArea2);

                    // 显示检测区域（调试用）
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_ImageReducedCheckArea, HDevWindowStack.GetActive());
                        HOperatorSet.DispObj(ho_ImageReducedCheckArea2, HDevWindowStack.GetActive());
                    }

                    // ========== 步骤4：针脚分割 ==========
                    // BinaryThreshold：自动二值化
                    // "max_separability" - 最大可分性方法，自动计算最佳阈值
                    // "light" - 提取亮部区域（针脚通常比背景亮）
                    HOperatorSet.BinaryThreshold(ho_ImageReducedCheckArea, out ho_Region3, "max_separability", "light", out hv_UsedThreshold);
                    HOperatorSet.BinaryThreshold(ho_ImageReducedCheckArea2, out ho_Region4, "max_separability", "light", out hv_UsedThreshold);

                    // FillUp：填充区域内部空洞
                    // 针脚内部可能有暗点，填充后形成完整区域
                    HOperatorSet.FillUp(ho_Region3, out ho_RegionFillUp);
                    HOperatorSet.FillUp(ho_Region4, out ho_RegionFillUp2);

                    // OpeningCircle：开运算（先腐蚀后膨胀）
                    // 作用：去除小噪点，断开细小连接
                    // 参数3/4是圆形结构元素半径
                    HOperatorSet.OpeningCircle(ho_RegionFillUp, out ho_RegionOpening, 3);
                    HOperatorSet.OpeningCircle(ho_RegionFillUp2, out ho_RegionOpening2, 4);

                    // ClosingCircle：闭运算（先膨胀后腐蚀）
                    // 作用：连接断裂部分，平滑边缘
                    HOperatorSet.ClosingCircle(ho_RegionFillUp, out ho_RegionClosing, 3);
                    HOperatorSet.ClosingCircle(ho_RegionFillUp2, out ho_RegionClosing2, 4);

                    // 显示处理结果（调试用）
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetColor(HDevWindowStack.GetActive(), "blue");
                        HOperatorSet.DispObj(ho_RegionOpening, HDevWindowStack.GetActive());
                        HOperatorSet.DispObj(ho_RegionOpening2, HDevWindowStack.GetActive());
                        HOperatorSet.DispObj(ho_RegionClosing, HDevWindowStack.GetActive());
                        HOperatorSet.DispObj(ho_RegionClosing2, HDevWindowStack.GetActive());
                    }

                    // ========== 步骤5：连通域分析和筛选 ==========
                    // Connection：将区域拆分为独立的连通域
                    // 每个针脚成为一个独立的连通域
                    HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions);
                    HOperatorSet.Connection(ho_RegionClosing2, out ho_ConnectedRegions2);

                    // 计算连通域面积和中心（用于调试）
                    HOperatorSet.AreaCenter(ho_ConnectedRegions, out hv_Area, out hv_Row, out hv_Column);
                    HOperatorSet.AreaCenter(ho_ConnectedRegions2, out hv_Area, out hv_Row, out hv_Column);

                    // SelectShape：按形状特征筛选区域
                    // "area" - 按面积筛选
                    // "and" - 同时满足上下限
                    // 500-6000 - 面积范围（像素），根据实际针脚大小调整
                    HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area", "and", 500, 6000);
                    HOperatorSet.SelectShape(ho_ConnectedRegions2, out ho_SelectedRegions2, "area", "and", 500, 6000);

                    // CountObj：统计区域数量
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_Number);
                    HOperatorSet.CountObj(ho_SelectedRegions2, out hv_Number2);

                    // 保存针脚数量
                    result.PinCount = hv_Number.I;
                    result.PinCount2 = hv_Number2.I;

                    // ========== 步骤6：合格判定与缺陷分类 ==========
                    bool area1Valid = result.PinCount >= _pinCountMin && result.PinCount <= _pinCountMax;
                    bool area2Valid = result.PinCount2 >= _pinCount2Min && result.PinCount2 <= _pinCount2Max;

                    if (area1Valid && area2Valid)
                    {
                        // 细粒度几何校验：歪脚检测 (引脚倾角与长宽比)
                        CheckPinGeometry(ho_SelectedRegions, out bool bent1, out string d1);
                        CheckPinGeometry(ho_SelectedRegions2, out bool bent2, out string d2);

                        if (bent1 || bent2)
                        {
                            result.IsOK = false;
                            result.DefectType = PinDefectType.BentPin;
                            result.DefectDescription = $"引脚弯曲歪斜超差: {d1} {d2}".Trim();
                            result.ResultText = "NG-歪脚";
                        }
                        else
                        {
                            result.IsOK = true;
                            result.DefectType = PinDefectType.None;
                            result.DefectDescription = "合格";
                            result.ResultText = "OK";
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispText(HDevWindowStack.GetActive(), result.ResultText, "window", 20, 20, result.IsOK ? "green" : "red", new HTuple(), new HTuple());
                        }
                    }
                    else
                    {
                        result.IsOK = false;
                        // 连锡短路 vs 缺脚判定
                        bool bridge1 = CheckSolderBridge(ho_ConnectedRegions);
                        bool bridge2 = CheckSolderBridge(ho_ConnectedRegions2);

                        if (bridge1 || bridge2)
                        {
                            result.DefectType = PinDefectType.SolderBridge;
                            result.DefectDescription = $"引脚连锡粘连短路: 区域1={result.PinCount}/4, 区域2={result.PinCount2}/4";
                            result.ResultText = "NG-连锡";
                        }
                        else
                        {
                            result.DefectType = PinDefectType.MissingPin;
                            result.DefectDescription = $"引脚缺失: 区域1={result.PinCount}/4, 区域2={result.PinCount2}/4";
                            result.ResultText = "NG-缺脚";
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispText(HDevWindowStack.GetActive(), result.ResultText, "window", 20, 20, "red", new HTuple(), new HTuple());
                        }
                    }
                }
                else
                {
                    // 未匹配到目标
                    result.IsOK = false;
                    result.DefectType = PinDefectType.MatchFailed;
                    result.DefectDescription = "未在视野中识别到芯片";
                    result.ResultText = "No found";
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispText(HDevWindowStack.GetActive(), "No found", "window", 20, 20, "black", new HTuple(), new HTuple());
                    }
                }

                // 返回处理后的图像（用于界面显示）
                // 【修复】Clone后ho_GrayImage在finally中释放，DisplayImage是独立副本
                result.DisplayImage = ho_GrayImage?.Clone();
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.DefectType = PinDefectType.Exception;
                result.DefectDescription = $"算法异常: {ex.Message}";
                result.ErrorMessage = ex.Message;
                result.ResultText = "检测出错";
                MessageBox.Show($"检测失败：{ex.Message}\n堆栈:{ex.StackTrace}");
                return result;
            }
            finally
            {
                // ========== 资源释放 ==========
                // 释放所有临时Halcon对象，避免内存泄漏
                // ho_GrayImage 必须在finally中释放，否则每次调用泄漏一整张灰度图
                ho_GrayImage?.Dispose();
                ho_ContoursAffineTrans?.Dispose();
                ho_Region?.Dispose();
                ho_Region2?.Dispose();
                ho_checkRectXid?.Dispose();
                ho_checkRectXid2?.Dispose();
                ho_ImageReducedCheckArea?.Dispose();
                ho_ImageReducedCheckArea2?.Dispose();
                ho_Region3?.Dispose();
                ho_Region4?.Dispose();
                ho_RegionFillUp?.Dispose();
                ho_RegionFillUp2?.Dispose();
                ho_RegionOpening?.Dispose();
                ho_RegionOpening2?.Dispose();
                ho_RegionClosing?.Dispose();
                ho_RegionClosing2?.Dispose();
                ho_ConnectedRegions?.Dispose();
                ho_ConnectedRegions2?.Dispose();
                ho_SelectedRegions?.Dispose();
                ho_SelectedRegions2?.Dispose();
                hv_fRow?.Dispose();
                hv_fColumn?.Dispose();
                hv_Angle?.Dispose();
                hv_Score?.Dispose();
                hv_HomMat2D?.Dispose();
                hv_HomMat2DCheck?.Dispose();
                hv_MRow?.Dispose();
                hv_MColumn?.Dispose();
                hv_MPhi?.Dispose();
                hv_UsedThreshold?.Dispose();
                hv_Area?.Dispose();
                hv_Row?.Dispose();
                hv_Column?.Dispose();
                hv_Number?.Dispose();
                hv_Number2?.Dispose();
            }

            return result;
        }
        #endregion

        #region 缺陷细分辅助算法
        /// <summary>
        /// 检查连通域中是否存在连锡粘连短路
        /// </summary>
        private bool CheckSolderBridge(HObject regions)
        {
            if (regions == null || !regions.IsInitialized()) return false;
            HTuple area = null, row = null, col = null;
            try
            {
                HOperatorSet.AreaCenter(regions, out area, out row, out col);
                for (int i = 0; i < area.Length; i++)
                {
                    // 正常单引脚面积通常在 1000~2800 像素，两脚粘连合并面积通常 > 3800
                    if (area[i].D > 3800.0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                area?.Dispose();
                row?.Dispose();
                col?.Dispose();
            }
        }

        /// <summary>
        /// 检查筛选后的引脚集合是否存在倾角或长宽比扭曲（歪脚判定）
        /// </summary>
        private void CheckPinGeometry(HObject selectedRegions, out bool isBent, out string detail)
        {
            isBent = false;
            detail = "";
            if (selectedRegions == null || !selectedRegions.IsInitialized()) return;
            HTuple row = null, col = null, phi = null, length1 = null, length2 = null;
            try
            {
                HOperatorSet.SmallestRectangle2(selectedRegions, out row, out col, out phi, out length1, out length2);
                for (int i = 0; i < phi.Length; i++)
                {
                    double l1 = length1[i].D;
                    double l2 = Math.Max(1.0, length2[i].D);
                    double ratio = l1 / l2;
                    // 引脚长宽比严重偏离设计公差 (正常范围 1.5 ~ 5.5)
                    if (ratio < 1.3 || ratio > 6.5)
                    {
                        isBent = true;
                        detail = $"引脚#{i + 1}长宽比失真({ratio:F1})";
                        return;
                    }
                }
            }
            catch
            {
                // 忽略几何计算轻微容错
            }
            finally
            {
                row?.Dispose();
                col?.Dispose();
                phi?.Dispose();
                length1?.Dispose();
                length2?.Dispose();
            }
        }
        #endregion
    }
}
