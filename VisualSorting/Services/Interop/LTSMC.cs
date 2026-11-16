using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Leadshine
{
    /// <summary>
    /// 雷赛LTSMC运动控制卡DLL的P/Invoke声明
    /// 封装LTSMC.dll中的所有导出函数，供C#代码调用
    /// 
    /// 功能分类：
    /// - 连接管理：smc_board_init/smc_board_close（控制卡连接/断开）
    /// - 电机控制：smc_vmove/smc_stop/smc_write_sevon_pin（传送带启停、电机使能）
    /// - 速度设置：smc_set_profile_unit/smc_change_speed（运动参数配置、调速）
    /// - IO操作：smc_read_inbit/smc_write_outbit（数字输入读取、数字输出控制）
    /// - 位置控制：smc_pmove_unit/smc_set_position_unit（绝对/相对运动、位置设置）
    /// - 回零操作：smc_home_move/smc_set_homemode（回零运动、回零模式配置）
    /// - 连续插补：smc_conti_line_unit/smc_conti_arc_move（直线/圆弧插补）
    /// - CANopen：nmcs_*系列函数（CAN总线从站控制）
    /// 
    /// 本项目使用的关键函数：
    /// - smc_board_init：连接控制卡（MotionControlService.Connect）
    /// - smc_write_sevon_pin：电机使能（MotionControlService.Sevon）
    /// - smc_vmove：传送带连续运动（MotionControlService.Vmove）
    /// - smc_stop：停止运动（MotionControlService.Vstop）
    /// - smc_set_profile_unit：设置速度参数（MotionControlService.ChangeSpeed）
    /// - smc_read_inbit：读取传感器输入（SensorService.ReadSensorState）
    /// - smc_write_outbit：控制电磁阀输出（SolenoidValueService.OpenValue/CloseValue）
    /// 
    /// 注意：此文件为第三方DLL的P/Invoke声明，不应修改函数签名
    /// </summary>
    public static class LTSMC
    {
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_connect_timeout(uint time_ms);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_connect_status(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_send_recv_timeout(uint SendTime_ms, uint RecvTime_ms);
		[DllImport("LTSMC.dll")]
        public static extern short smc_board_init(ushort ConnectNo, ushort type, string pconnectstring, uint dwBaudRate);
        [DllImport("LTSMC.dll")]
        public static extern short smc_board_init_ex(ushort ConnectNo, ushort ConnectType, string pconnectstring, uint dwBaudRate, uint dwByteSize, uint dwParity, uint dwStopBits);
		[DllImport("LTSMC.dll")]
        public static extern short smc_board_close(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_soft_reset(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_board_reset(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_debug_mode(ushort mode, string FileName);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_debug_mode(ref ushort mode, byte[] FileName);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_connect_debug_time(ushort ConnectNo, uint time_s);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_card_version(ushort ConnectNo, ref uint CardVersion);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_card_soft_version(ushort ConnectNo, ref uint FirmID, ref uint SubFirmID);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_card_lib_version(ref uint LibVer);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_release_version(ushort ConnectNo, byte[] ReleaseVersion);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_total_axes(ushort ConnectNo, ref uint TotalAxis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_total_ionum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_total_adcnum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);
		[DllImport("LTSMC.dll")]
        public static extern short smc_format_flash(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_rtc_get_time(ushort ConnectNo, ref int year, ref int month, ref int day, ref int hour, ref int min, ref int sec);
		[DllImport("LTSMC.dll")]
        public static extern short smc_rtc_set_time(ushort ConnectNo, int year, int month, int day, int hour, int min, int sec);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_ipaddr(ushort ConnectNo, byte[] IpAddr);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_ipaddr(ushort ConnectNo, byte[] IpAddr);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_com(ushort ConnectNo, ushort com, uint dwBaudRate, ushort wByteSize, ushort wParity, ushort wStopBits);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_com(ushort ConnectNo, ushort com, ref uint dwBaudRate, ref ushort wByteSize, ref ushort wParity, ref ushort dwStopBits);
		[DllImport("LTSMC.dll")]
        public static extern short smc_write_sn(ushort ConnectNo, UInt64 sn);
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_sn(ushort ConnectNo, ref UInt64 sn);
		[DllImport("LTSMC.dll")]
        public static extern short smc_write_sn_numstring(ushort ConnectNo,  string sn_str);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_sn_numstring(ushort ConnectNo, byte[] sn_str);
		[DllImport("LTSMC.dll")]
        public static extern short smc_write_password(ushort ConnectNo, string str_pass);
        [DllImport("LTSMC.dll")]
        public static extern short smc_check_password(ushort ConnectNo, string str_pass);
		[DllImport("LTSMC.dll")]
        public static extern short smc_enter_password(ushort ConnectNo, string str_pass);
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_password(ushort ConnectNo, string spassold, string spass);
		[DllImport("LTSMC.dll")]
        public static extern short smc_download_parafile(ushort ConnectNo, byte[] FileName);
        [DllImport("LTSMC.dll")]
        public static extern short smc_upload_parafile(ushort ConnectNo, byte[] FileName);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_el_mode(ushort ConnectNo, ushort axis, ushort enable, ushort el_logic, ushort el_mode); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_el_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort el_logic, ref ushort el_mode); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_emg_mode(ushort ConnectNo, ushort axis, ushort enable, ushort emg_logic); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_emg_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort emg_logic); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_softlimit_unit(ushort ConnectNo, ushort axis, ushort enable, ushort source_sel, ushort SL_action, double N_limit, double P_limit);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_softlimit_unit(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort source_sel, ref ushort SL_action, ref double N_limit, ref double P_limit);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pulse_outmode(ushort ConnectNo, ushort axis, ushort outmode);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pulse_outmode(ushort ConnectNo, ushort axis, ref ushort outmode);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pulse_fractional_number(ushort ConnectNo, ushort axis, ushort fractional_number);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_pulse_fractional_number(ushort ConnectNo, ushort axis, ref ushort fractional_number);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_equiv(ushort ConnectNo, ushort axis, double equiv);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_equiv(ushort ConnectNo, ushort axis, ref double equiv);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_backlash_unit(ushort ConnectNo, ushort axis, double backlash);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_backlash_unit(ushort ConnectNo, ushort axis, ref double backlash);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_axis_io_map(ushort ConnectNo, ushort Axis, ushort IoType, ushort MapIoType, ushort MapIoIndex, double Filter);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_axis_io_map(ushort ConnectNo, ushort Axis, ushort IoType, ref ushort MapIoType, ref ushort MapIoIndex, ref double Filter);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_profile_unit(ushort ConnectNo, ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_profile_unit(ushort ConnectNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_profile_unit_acc(ushort ConnectNo, ushort axis, double Min_Vel, double Max_Vel, double acc, double dec, double Stop_Vel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_profile_unit_acc(ushort ConnectNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double acc, ref double dec, ref double Stop_Vel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_s_profile(ushort ConnectNo, ushort axis, ushort s_mode, double s_para);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_s_profile(ushort ConnectNo, ushort axis, ushort s_mode, ref double s_para);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_dec_stop_time(ushort ConnectNo, ushort axis, double time);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_dec_stop_time(ushort ConnectNo, ushort axis, ref double time);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pmove_unit(ushort ConnectNo, ushort axis, double Dist, ushort posi_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_vmove(ushort ConnectNo, ushort axis, ushort dir);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_change_speed(ushort ConnectNo, ushort axis, double Curr_Vel, double Taccdec);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_reset_target_position_unit(ushort ConnectNo, ushort axis, double New_Pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_update_target_position_unit(ushort ConnectNo, ushort axis, double New_Pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_home_pin_logic(ushort ConnectNo, ushort axis, ushort org_logic, double filter); 	
        [DllImport("LTSMC.dll")]
		public static extern short smc_get_home_pin_logic(ushort ConnectNo, ushort axis, ref ushort org_logic, ref double filter); 	     
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ez_mode(ushort ConnectNo, ushort axis, ushort ez_logic, ushort ez_mode, double filter);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ez_mode(ushort ConnectNo, ushort axis, ref ushort ez_logic, ref ushort ez_mode, ref double filter);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_homemode(ushort ConnectNo, ushort axis, ushort home_dir, double vel_mode, ushort mode, ushort pos_source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_homemode(ushort ConnectNo, ushort axis, ref ushort home_dir, ref double vel_mode, ref ushort home_mode, ref ushort pos_source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_homespeed_unit(ushort ConnectNo, ushort axis, double homespeed);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_homespeed_unit(ushort ConnectNo, ushort axis, ref double homespeed);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_home_profile_unit(ushort ConnectNo, ushort axis, double Low_Vel, double High_Vel, double Tacc, double Tdec);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_home_profile_unit(ushort ConnectNo, ushort axis, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_el_home(ushort ConnectNo, ushort axis, ushort mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_home_position_unit(ushort ConnectNo, ushort axis, ushort enable, double position);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_home_position_unit(ushort ConnectNo, ushort axis, ref ushort enable, ref double position);
		[DllImport("LTSMC.dll")]
        public static extern short smc_home_move(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_home_result(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_inbit(ushort ConnectNo, ushort bitno); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_outbit(ushort ConnectNo, ushort bitno, ushort on_off); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_outbit(ushort ConnectNo, ushort bitno);  	
        [DllImport("LTSMC.dll")]
        public static extern uint smc_read_inport(ushort ConnectNo, ushort portno); 	
        [DllImport("LTSMC.dll")]
        public static extern uint smc_read_outport(ushort ConnectNo, ushort portno); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_outport(ushort ConnectNo, ushort portno, uint outport_val);  	
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_inbit_ex(ushort ConnectNo, ushort bitno, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_outbit_ex(ushort ConnectNo, ushort bitno, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_inport_ex(ushort ConnectNo, ushort portno, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_outport_ex(ushort ConnectNo, ushort portno, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_emg_stop(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_check_done(ushort ConnectNo, ushort axis);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_stop(ushort ConnectNo, ushort axis, ushort stop_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_write_sevon_pin(ushort ConnectNo, ushort axis, ushort on_off); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_sevon_pin(ushort ConnectNo, ushort axis); 	
		[DllImport("LTSMC.dll")]
        public static extern short smc_write_erc_pin(ushort ConnectNo, ushort axis, ushort on_off); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_erc_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_alarm_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_inp_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_org_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_elp_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_eln_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_emg_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_ez_pin(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_rdy_pin(ushort ConnectNo, ushort axis); 
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_position_unit(ushort ConnectNo, ushort axis, double pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_position_unit(ushort ConnectNo, ushort axis, ref double pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_workpos_unit(ushort ConnectNo, ushort axis, double pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_workpos_unit(ushort ConnectNo, ushort axis, ref double pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_compare_set_config(ushort ConnectNo, ushort axis, ushort enable, ushort cmp_source); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_config(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort cmp_source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_clear_points(ushort ConnectNo, ushort axis); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_add_point_unit(ushort ConnectNo, ushort axis, double pos, ushort dir, ushort action, uint actpara); 
		[DllImport("LTSMC.dll")]
        public static extern short smc_compare_add_point_cycle(ushort ConnectNo, ushort axis, double pos, ushort dir, uint bitno, uint cycle, ushort level);
		[DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_current_point_unit(ushort ConnectNo, ushort axis, ref double pos); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_points_runned(ushort ConnectNo, ushort axis, ref int pointNum); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_points_remained(ushort ConnectNo, ushort axis, ref int pointNum); 
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_axes(ushort ConnectNo, ref uint TotalAxis); 	
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_ionum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_enable(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_disable(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_io_out(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_io_out(ushort ConnectNo, ushort axis, int iostate);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_io_in(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_type(ushort ConnectNo, ushort axis, ref ushort Axis_Type);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_axis_io_status(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_card_errcode(ushort ConnectNo, ref int Errcode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_clear_card_errcode(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_errcode(ushort ConnectNo, ushort PortNum, ref int errcode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_clear_errcode(ushort ConnectNo, ushort PortNum);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_errcode(ushort ConnectNo, ushort axis, ref int errcode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inbit_extern(ushort ConnectNo, ushort Channel, ushort NoteID, ushort IoBit, ref ushort IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inport_extern(ushort ConnectNo, ushort Channel, ushort NoteID, ushort PortNo, ref int IoValue);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outbit_extern(ushort ConnectNo, ushort PortNo, ushort NodeID, ushort BitNo, ushort IoValue);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outport_extern(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, uint  IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outbit_extern(ushort ConnectNo, ushort PortNo, ushort NodeID, ushort BitNo, ref ushort IoValue);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outport_extern(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, ref int IoValue);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_reset_canopen(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_LostHeartbeat_Nodes(ushort ConnectNo, ushort PortNum, ushort[] NodeID, ref ushort NodeNum);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_EmergeneyMessege_Nodes(ushort ConnectNo, ushort PortNum, uint[] NodeMsg, ref ushort MsgNum);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_SendNmtCommand(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort NmtCommand);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_alarm_clear(ushort ConnectNo, ushort PortNum, ushort nodenum);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_syn_move_unit(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, double[] Position, ushort[] PosiMode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_adcnum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_reset_etc(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_stop_etc(ushort ConnectNo, ushort[] ETCState);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_state_machine(ushort ConnectNo, ushort axis, ref ushort Axis_StateMachine);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_node_address(ushort ConnectNo, ushort axis, ref ushort SlaveAddr, ref ushort Sub_SlaveAddr);
		[DllImport("LTSMC.dll")]
        public static extern short smc_check_done_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_axis_io_status(ushort ConnectNo, ushort axis);
        [DllImport("LTSMC.dll")]
        public static extern short smc_axis_io_enable_status(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_axis_run_mode(ushort ConnectNo, ushort axis, ref ushort run_mode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_run_mode(ushort ConnectNo, ushort axis, ushort run_mode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outbit(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort IoBit, ushort IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outbit(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort IoBit, ref ushort IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inbit(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort IoBit, ref ushort IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outport(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, int IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outport(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, ref int IoValue);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inport(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, ref int IoValue);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_slaves(ushort ConnectNo, ushort PortNum, ref ushort TotalSlaves);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_node_od(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, uint value);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_node_od(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref uint value);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_node_od_float(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, float value);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_node_od_float(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref float value);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_node_od_pbyte(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort bytes, byte[] value);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_node_od_pbyte(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort bytes, byte[] value);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_cycletime(ushort ConnectNo, ushort PortNo, int CycleTime);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_cycletime(ushort ConnectNo, ushort PortNo, ref int CycleTime);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_offset_pos(ushort ConnectNo, ushort axis, double offset_pos);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_offset_pos(ushort ConnectNo, ushort axis, ref double offset_pos);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_slave_output_retain(ushort ConnectNo, ushort Enable);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_slave_output_retain(ushort ConnectNo, ref ushort Enable);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_home_profile(ushort ConnectNo, ushort axis, ushort home_mode, double High_Vel, double Low_Vel, double Tacc, double Tdec, double offsetpos);
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_home_profile(ushort ConnectNo, ushort axis, ref ushort home_mode, ref double High_Vel, ref double Low_Vel, ref double Tacc, ref double Tdec, ref double offsetpos);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_torque_move(ushort CardNo, ushort axis, int Torque, ushort PosLimitValid, double PosLimitValue, ushort PosMode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_change_torque(ushort CardNo, ushort axis, int Torque);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_torque(ushort CardNo, ushort axis, ref int Torque);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_etc_el_stop_mode(ushort ConnectNo, ushort axis, ushort el_control_mode, double diff_pos, int filter);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_sync_pos_change_mode (ushort ConnectNo, ushort portno, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_master_state(ushort ConnectNo, UInt32[] States);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_stop_io_map_virtual(ushort ConnectNo, ushort VirtualBitNo, ushort MapIoType, ushort MapIoIndex, ushort MapIoLogic, ushort MapIoFilter);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_stop_io_map_virtual(ushort ConnectNo, ushort VirtualBitNo, ref ushort MapIoType, ref ushort MapIoIndex, ref ushort MapIoLogic, ref ushort MapIoFilter);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_stop_extra_pdo_map_virtual(ushort ConnectNo, ushort VirtualExtraPdo, ushort MapExtraPdoType, ushort MapExtraPdoAddress, ushort MapDataLen, UInt32 MapMaxData, UInt32 MapMinData);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_stop_extra_pdo_map_virtual(ushort ConnectNo, ushort VirtualExtraPdo, ref ushort MapExtraPdoType, ref ushort MapExtraPdoAddress, ref ushort MapDataLen, ref UInt32 MapMaxData, ref UInt32 MapMinData);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_extra_stop_mode(ushort ConnectNo, ushort Axis, ushort VirtualIoNum, ushort[] VirtualIoList, ushort VirtualExtraPdoNum, ushort[] VirtualExtraPdoList, ushort CmpMode, ushort StopMode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_axis_io_status_ex(ushort ConnectNo, ushort axis, ref int state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_axis_io_enable_status_ex(ushort ConnectNo, ushort axis, ref int state);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_current_fieldbus_state_info(ushort ConnectNo, ushort Channel, ushort[] Axis, ushort[] ErrorType,ushort[] SlaveAddr, int[] ErrorFieldbusCode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_detail_fieldbus_state_info(ushort ConnectNo, ushort Channel, ushort ReadErrorNum, ref ushort TotalNum, ref int ActualNum, ref ushort Axis, ref ushort ErrorType, ref ushort SlaveAddr, ref int ErrorFieldbusCode);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_error_checktimes(ushort ConnectNo, ushort channel, ushort checktimes);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_get_error_checktimes(ushort ConnectNo, ushort channel, ref int checktimes);
		[DllImport("LTSMC.dll")]
        public static extern short smc_userlib_loadlibrary(ushort ConnectNo, byte[] pLibname);
		[DllImport("LTSMC.dll")]
        public static extern short smc_userlib_set_parameter(ushort ConnectNo, int type, byte[] pParameter, int length);
		[DllImport("LTSMC.dll")]
        public static extern short smc_userlib_get_parameter(ushort ConnectNo, int type, byte[] pParameter, int length);
		[DllImport("LTSMC.dll")]
        public static extern short smc_userlib_imd_stop(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_position_range_limit(ushort ConnectNo, ushort Axis, ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_position_range_limit(ushort ConnectNo, ushort Axis, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_action_event(ushort ConnectNo, uint event_mask);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_action_event(ushort ConnectNo, ref uint event_mask);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_enable(ushort ConnectNo, double timer_period, uint enable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_enable(ushort ConnectNo, ref double timer_period, ref uint enable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_watchdog_timer(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_action_event_extern(ushort ConnectNo, ushort index, ushort event_mask);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_action_event_extern(ushort ConnectNo, ushort index, ref ushort event_mask);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_enable_extern(ushort ConnectNo, ushort index, double timer_period, ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_enable_extern(ushort ConnectNo, ushort index, double timer_period, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_reset_watchdog_timer_extern(ushort ConnectNo, ushort index);
		[DllImport("LTSMC.dll")]
        public static extern short smc_enable_leadscrew_comp(ushort ConnectNo, ushort axis, ushort enable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_config_unit(ushort ConnectNo, ushort axis, ushort n, double startpos, double lenpos, double[] pCompPos, double[] pCompNeg);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_config_unit(ushort ConnectNo, ushort axis, ref ushort n, ref double startpos, ref double lenpos, double[] pCompPos, double[] pCompNeg);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_position_ex_unit(ushort ConnectNo, ushort axis, ref double pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_gear_follow_profile(UInt16 ConnectNo, UInt16 axis, UInt16 enable, UInt16 master_axis, double ratio);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_gear_follow_profile(UInt16 ConnectNo, UInt16 axis, ref UInt16 enable, ref UInt16 master_axis, ref double ratio);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_grant_error_protect_unit(UInt16 ConnectNo, UInt16 axis, UInt16 enable, double dstp_error, double emg_error);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_grant_error_protect_unit(UInt16 ConnectNo, UInt16 axis, ref UInt16 enable, ref double dstp_error, ref double emg_error);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_config_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref double axis_segment, double[] CompPos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_config_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref ushort axis_segment, double[] CompPos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_angle_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, double[] axis_start_pos, double[] axis_length, double angle);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_angle_unit (ushort ConnectNo, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref double angle);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_enable(ushort ConnectNo, ushort axis, ushort mode, ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_enable(ushort ConnectNo, ushort axis, ref ushort mode, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_config_unit_ex(ushort ConnectNo, ushort table_index, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref ushort axis_segment, ref double value);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_config_unit_ex(ushort ConnectNo, ushort table_index, ref ushort axis, ref double axis_start_pos, ref double axis_length, ref ushort axis_segment, double value);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_angle_unit_ex(ushort ConnectNo, ushort table_index, ushort comp_axis, ushort[] axis, double[] axis_start_pos, double[] axis_length, double angle);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_angle_unit_ex(ushort ConnectNo, ushort table_index, ushort[] comp_axis, ushort[] axis, double[] axis_start_pos, double[] axis_length, ref double angle);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_enable_ex(ushort ConnectNo, ushort table_index, ushort mode, ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_enable_ex(ushort ConnectNo, ushort table_index, ref ushort mode, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_force_set_position(ushort ConnectNo, ushort Crd, ushort axis_num, ushort[] axis_list, double[] position);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_modulo_profile(ushort ConnectNo, ushort axis, ushort enable, double Modulo_Vel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_modulo_profile(ushort ConnectNo, ushort axis, ref ushort enable, ref double Modulo_Vel);
		[DllImport("LTSMC.dll")]
        public static extern short nmcs_set_data_offset_time(ushort ConnectNo, int offset_us);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_acuate_angle_config_params(ushort ConnectNo, ushort Crd, double acuate_angle, double angle_trans_speed, ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_acuate_angle_config_params(ushort ConnectNo, ushort Crd, ref double acuate_angle, ref double angle_trans_speed, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_dxXYZLineBuff(ushort ConnectNo, ushort Crd, byte[] buff, ushort pack_num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_dfXYZLineBuff(ushort ConnectNo, ushort Crd, byte[] buff, ushort pack_num);
		[DllImport("LTSMC.dll")]
		public static extern short smc_delete_file(ushort ConnectNo, ushort FileType);
		[DllImport("LTSMC.dll")]
		public static extern short nmcs_ecat_read_slave_register(ushort ConnectNo, ushort wSlaveAddress, ushort wRegisterOffset, ushort wLen, byte[] pdwData);
		[DllImport("LTSMC.dll")]
		public static extern short nmcs_ecat_write_slave_register(ushort ConnectNo, ushort wSlaveAddress, ushort wRegisterOffset, ushort wLen, byte[] pdwData);
		[DllImport("LTSMC.dll")]
		public static extern short smc_cir_count_start(ushort ConnectNo, ushort Channel);
		[DllImport("LTSMC.dll")]
		public static extern short smc_get_cir_count_flag(ushort ConnectNo, ushort Channel, ref ushort Flag);
		[DllImport("LTSMC.dll")]
		public static extern short smc_cir_count_reset(ushort ConnectNo, ushort Channel);
		[DllImport("LTSMC.dll")]
		public static extern short smc_get_cir_count_value(ushort ConnectNo, ushort Channel, ref ushort Value);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_io_exactstop_ex(ushort ConnectNo, ushort axis, ushort ioNum, ushort[] ioList, ushort[] ioTypeList, ushort[] ioLogicList, ushort enable, ushort action, ushort move_dir);
		[DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_compare_config(ushort ConnectNo, ushort axis, ushort enable, ushort source);
		[DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_clear_point(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_add_point(ushort ConnectNo, double start_pos, double interval, ushort count);
		[DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_get_value(ushort ConnectNo, ref double pos, ref ushort val);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_open_list(ushort ConnectNo, ushort group, ushort axis_num, ref ushort axis_list);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_start_list(ushort ConnectNo, ushort group);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_close_list(ushort ConnectNo, ushort group);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_get_run_state(ushort ConnectNo, ushort group, ref ushort state, ref ushort enable, ref int stop_reason, ref ushort trig_phase, ref int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_stop_list(ushort ConnectNo, ushort group, ushort stopMode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_pause_list(ushort ConnectNo, ushort group, ushort stop_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_add_time_delay(ushort ConnectNo, ushort group, double Time_delay, int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_add_sigaxis_moveseg_data_ex(ushort ConnectNo, ushort group, ushort Axis, double Target_pos, int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_add_wait_event_data(ushort ConnectNo, ushort group, ushort evet, ushort num, ushort CompareOperator, double target_value, int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_add_trigger_data(ushort ConnectNo, ushort group, ushort mode, ushort num, double Target_Value, int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_m_set_profile_unit(ushort ConnectNo, ushort group, ushort axis, double start_vel, double max_vel, double tacc, double tdec, double stop_vel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_pack_on(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_pack_off(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_pack_flush(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_gap_cmp_space(ushort ConnectNo, ushort crd, double space);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_gap_cmp_space(ushort ConnectNo, ushort crd, ref double space);
		[DllImport("LTSMC.dll")]
        public static extern short smc_reverse_outbit(ushort ConnectNo, ushort bitno, double reverse_time);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_outbit_delay_reverse(ushort ConnectNo, ushort channel, ushort bitno, ushort level, double reverse_time, ushort outmode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_io_pwmoutput(ushort ConnectNo, ushort outbit, double fre, double duty, uint counts);
		[DllImport("LTSMC.dll")]
        public static extern short smc_clear_io_pwmoutput(ushort ConnectNo, ushort outbit);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_io_count_mode(ushort ConnectNo, ushort bitno, ushort mode, double filter);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_count_mode(ushort ConnectNo, ushort bitno, ref ushort mode, ref double filter);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_count_value(ushort ConnectNo, ushort bitno, uint CountValue);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_count_value(ushort ConnectNo, ushort bitno, ref uint CountValue);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_io_map_virtual(ushort ConnectNo, ushort bitno, ushort MapIoType, ushort MapIoIndex, double filter_time);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_map_virtual(ushort ConnectNo, ushort bitno, ref ushort MapIoType, ref ushort MapIoIndex, ref double filter_time);
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inbit_virtual(ushort ConnectNo, ushort bitno);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_io_dstp_mode(ushort ConnectNo, ushort axis, ushort enable, ushort logic); 
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_dstp_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort logic);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_alm_mode(ushort ConnectNo, ushort axis, ushort enable, ushort alm_logic, ushort alm_action);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_alm_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort alm_logic, ref ushort alm_action);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_inp_mode(ushort ConnectNo, ushort axis, ushort enable, ushort inp_logic);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_inp_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort inp_logic);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_sinp_param_unit(ushort ConnectNo, ushort axis, ushort enable, double inp_error, double inp_time_s);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_sinp_param_unit(ushort ConnectNo, ushort axis, ref ushort enable, ref double inp_error, ref double inp_time_s);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_sevon_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_erc_pin_ex(ushort ConnectNo, ushort axis, ref ushort state); 
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_alarm_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_inp_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_org_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_elp_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_eln_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_emg_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_rdy_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_set_mode(ushort ConnectNo, ushort hcmp, ushort cmp_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_mode(ushort ConnectNo, ushort hcmp, ref ushort cmp_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_set_config(ushort ConnectNo, ushort hcmp, ushort axis, ushort cmp_source, ushort cmp_logic, int time);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_config(ushort ConnectNo, ushort hcmp, ref ushort axis, ref ushort cmp_source, ref ushort cmp_logic, ref int time);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_add_point_unit(ushort ConnectNo, ushort hcmp, double cmp_pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_set_liner_unit(ushort ConnectNo, ushort hcmp, double Increment, int Count);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_liner_unit(ushort ConnectNo, ushort hcmp, ref double Increment, ref int Count);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_current_state_unit(ushort ConnectNo, ushort hcmp, ref int remained_points, ref double current_point, ref int runned_points);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_clear_points(ushort ConnectNo, ushort hcmp);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_set_mode(ushort ConnectNo, ushort hcmp, ushort fifo_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_get_mode(ushort ConnectNo, ushort hcmp, ushort[] fifo_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_get_state(ushort ConnectNo, ushort hcmp, short[] remained_points);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_add_point_unit(ushort ConnectNo, ushort hcmp, ushort num, double[] cmp_pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_clear_points(ushort ConnectNo, ushort hcmp);
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_add_table(ushort ConnectNo, ushort hcmp, ushort num, double[] cmp_pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_set_enable(ushort ConnectNo, ushort hcmp, ushort cmp_enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_enable(ushort ConnectNo, ushort hcmp, ref ushort cmp_enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_set_config_unit(ushort ConnectNo, ushort hcmp, ushort cmp_mode, ushort x_axis, ushort x_cmp_source, double x_cmp_error, ushort y_axis, ushort y_cmp_source, double y_cmp_error, ushort cmp_logic, int time);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_config_unit(ushort ConnectNo, ushort hcmp, ref ushort cmp_mode, ref ushort x_axis, ref ushort x_cmp_source, ref double x_cmp_error,  ref ushort y_axis, ref ushort y_cmp_source, ref ushort y_cmp_error, ref ushort cmp_logic, ref int time);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_set_pwmoutput(ushort ConnectNo, ushort hcmp, ushort pwm_enable, double duty, double freq, ushort pwm_number);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_pwmoutput(ushort ConnectNo, ushort hcmp, ref ushort pwm_enable, ref double duty, ref double freq, ref ushort pwm_number);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_add_point_unit(ushort ConnectNo, ushort hcmp, double x_cmp_pos, double y_cmp_pos, ushort cmp_outbit);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_current_state_unit(ushort ConnectNo, ushort hcmp, ref int remained_points, ref double x_current_point, ref double y_current_point, ref int runned_points, ref ushort current_state, ref ushort current_outbit); 
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_clear_points(ushort ConnectNo, ushort hcmp);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_force_output(ushort ConnectNo, ushort hcmp, ushort outbit);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_set_mode(ushort ConnectNo, ushort hcmp, ushort fifo_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_get_mode(ushort ConnectNo, ushort hcmp, ushort[] fifo_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_get_state(ushort ConnectNo, ushort hcmp, long[] remained_points); 
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_add_point_unit(ushort ConnectNo, ushort hcmp, ushort num, double[] x_cmp_pos, double[] y_cmp_pos, ushort cmp_outbit);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_clear_points(ushort ConnectNo, ushort hcmp);
		[DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_add_table_unit(ushort ConnectNo, ushort hcmp, ushort num, double[] x_cmp_pos, double[] y_cmp_pos, ushort outbit);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_homelatch_mode(ushort ConnectNo, ushort axis, ushort enable, ushort logic, ushort source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_homelatch_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort logic, ref ushort source);
        [DllImport("LTSMC.dll")]
        public static extern int smc_get_homelatch_flag(ushort ConnectNo, ushort axis);
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_homelatch_flag(ushort ConnectNo, ushort axis);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_homelatch_value_unit(ushort ConnectNo, ushort axis, ref double value);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_ezlatch_mode(ushort ConnectNo, ushort axis, ushort enable, ushort logic, ushort source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ezlatch_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort logic, ref ushort source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ezlatch_flag(ushort ConnectNo, ushort axis);
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_ezlatch_flag(ushort ConnectNo, ushort axis);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ezlatch_value_unit(ushort ConnectNo, ushort axis, ref double pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_ltc_mode(ushort ConnectNo, ushort axis, ushort ltc_logic, ushort ltc_mode, double filter); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ltc_mode(ushort ConnectNo, ushort axis, ref ushort ltc_logic, ref ushort ltc_mode, ref double filter);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_latch_mode(ushort ConnectNo, ushort axis, ushort latchmode, ushort latch_source, ushort latch_channel); 	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_latch_mode(ushort ConnectNo, ushort axis, ref ushort latchmode, ref ushort latch_source, ref ushort latch_channel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_latch_flag(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_reset_latch_flag(ushort ConnectNo, ushort axis);     
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_latch_value_unit(ushort ConnectNo, ushort axis, ref double value);
		[DllImport("LTSMC.dll")]
        public static extern short smc_ltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_mode, ushort ltc_logic, double filter);
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_mode, ref ushort ltc_logic, ref double filter);
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_reset(ushort ConnectNo, ushort latch);
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_number(ushort ConnectNo, ushort latch,  ushort axis, ref int  number);     
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_value_unit(ushort ConnectNo, ushort latch,  ushort axis, ref double value);       
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_enable, ushort ltc_mode, ushort ltc_inbit, ushort ltc_logic, double filter);
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_enable, ref ushort ltc_mode, ref ushort ltc_inbit, ref ushort ltc_logic, ref double filter);
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_reset(ushort ConnectNo, ushort latch);
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_number(ushort ConnectNo, ushort latch, ushort axis, ref int number);
		[DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_value_unit(ushort ConnectNo, ushort latch, ushort axis, ref double value);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_ain_action(ushort ConnectNo, ushort channel, ushort mode, double fvoltage, ushort action, double actpara);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ain_action(ushort ConnectNo, ushort channel, ref ushort mode, ref double fvoltage, ref ushort action, ref double actpara);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ain_state(ushort ConnectNo, ushort channel);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ain_state(ushort ConnectNo, ushort channel);
		[DllImport("LTSMC.dll")]
        public static extern double smc_get_ain(ushort ConnectNo, ushort channel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_ain_extern(ushort ConnectNo, ushort channel, ref double Vout);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_ad_input(ushort ConnectNo, ushort da_no, ref double Vout);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ad_input_all(ushort ConnectNo, ref double Vout);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_da_output(ushort ConnectNo, ushort da_no, double Vout);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_da_output(ushort ConnectNo, ushort da_no, ref double Vout);
		[DllImport("LTSMC.dll")]
        public static extern short smc_download_file(ushort ConnectNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_memfile(ushort ConnectNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ushort filetype);
        [DllImport("LTSMC.dll")]
        public static extern short smc_upload_file(ushort ConnectNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
        [DllImport("LTSMC.dll")]
        public static extern short smc_upload_memfile(ushort ConnectNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ref uint puifilesize, ushort filetype);
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_file_to_ram(ushort ConnectNo, string pfilename, ushort filetype);
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_memfile_to_ram(ushort ConnectNo, byte[] pbuffer, uint buffsize, ushort filetype);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_progress(ushort ConnectNo, ref float progress);
		[DllImport("LTSMC.dll")]
        public static extern short smc_udisk_get_state(ushort ConnectNo, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_udisk_check_file(ushort ConnectNo, byte[] filename, int[] filesize, ushort filetype);
		[DllImport("LTSMC.dll")]
        public static extern short smc_udisk_get_first_file(ushort ConnectNo, byte[] filename, int[] filesize, int[] fileid, ushort filetype);
		[DllImport("LTSMC.dll")]
        public static extern short smc_udisk_get_next_file(ushort ConnectNo, byte[] filename, int[] filesize, int[] fileid, ushort filetype);
		[DllImport("LTSMC.dll")]
        public static extern short smc_udisk_copy_file(ushort ConnectNo, byte[] SrcFileName, byte[] DstFileName, ushort filetype, ushort mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_0x(ushort ConnectNo, ushort start, ushort inum, byte[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_modbus_0x(ushort ConnectNo, ushort start, ushort inum, byte[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_4x(ushort ConnectNo, ushort start, ushort inum, ushort[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_modbus_4x(ushort ConnectNo, ushort start, ushort inum, ushort[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_4x_float(ushort ConnectNo, ushort start, ushort inum, float[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_modbus_4x_float(ushort ConnectNo, ushort start, ushort inum, float[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_4x_int(ushort ConnectNo, ushort start, ushort inum, int[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg(ushort ConnectNo, ushort start, ushort inum, byte[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg(ushort ConnectNo, ushort start, ushort inum, byte[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_byte(ushort ConnectNo, uint start, uint inum, byte[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_byte(ushort ConnectNo, uint start, uint inum, byte[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_float(ushort ConnectNo, uint start, uint inum, float[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_float(ushort ConnectNo, uint start, uint inum, float[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_int(ushort ConnectNo, uint start, uint inum, int[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_int(ushort ConnectNo, uint start, uint inum, int[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_short(ushort ConnectNo, uint start, uint inum, short[] pdata);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_short(ushort ConnectNo, uint start, uint inum, short[] pdata);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_array(ushort ConnectNo, string name, uint index, long[] var, ref int num);
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_array(ushort ConnectNo, string name, uint index, long[] var, int num);
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_var(ushort ConnectNo, string varstring, long[] var, ref int num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_modify_var(ushort ConnectNo, string varstring, long[] var, int varnum);
		[DllImport("LTSMC.dll")]
        public static extern short smc_write_array(ushort ConnectNo,  string name, long startindex, int[] var, long num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_array_ex(ushort ConnectNo, string name, uint index, double[] var, ref int num);
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_array_ex(ushort ConnectNo, string name, uint index, double[] var, int num);
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_var_ex(ushort ConnectNo, string varstring, double[] var, ref int num);
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_var_ex(ushort ConnectNo, string varstring, double[] var, int varnum);
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_array_ex(ushort ConnectNo, string name, uint startindex, double[] var, int num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_stringtype(ushort ConnectNo, string varstring, ref int m_Type, ref int num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_delete_file(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_run(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_stop(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_pause(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_step_run(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_step_over(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_continue_run(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_state(ushort ConnectNo, ref ushort State);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_current_line(ushort ConnectNo, ref int line);
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_break_info(ushort ConnectNo, int[] line, int linenum);
		[DllImport("LTSMC.dll")]
        public static extern short smc_basic_message(ushort ConnectNo, byte[] pbuff, uint uimax, ref uint puiread);
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_command(ushort ConnectNo, byte[] pszCommand, byte[] psResponse, uint uiResponseLength);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_check_file(ushort ConnectNo, byte[] pfilenameinControl, ref byte pbIfExist, ref uint pFileSize);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_first_file(ushort ConnectNo, byte[] pfilenameinControl, ref uint pFileSize);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_next_file(ushort ConnectNo, byte[] pfilenameinControl, ref uint pFileSize);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_start(ushort ConnectNo);
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_stop(ushort ConnectNo);
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_pause(ushort ConnectNo);
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_state(ushort ConnectNo, ref ushort state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_set_current_file(ushort ConnectNo, byte[] pFileName);
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_current_file(ushort ConnectNo, byte[] pFileName, ref short fileid);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_current_line(ushort ConnectNo, uint[] line, byte[] pCurLine);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_current_line(ushort ConnectNo, uint[] line, byte[] pCurLine);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_check_file_id(ushort ConnectNo, ushort fileid, string pFileName, ulong[] pFileSize, ulong[] pTotalLine);
		[DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_file_profile(ushort ConnectNo, ref uint maxfilenum, ref uint maxfilesize);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_current_speed_unit(ushort ConnectNo, ushort axis, ref double[] current_speed);
		[DllImport("LTSMC.dll")]
        public static extern int smc_get_target_position_unit(ushort ConnectNo, ushort axis, ref double pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_stop_reason(ushort ConnectNo, ushort axis, ref int StopReason);
		[DllImport("LTSMC.dll")]
        public static extern short smc_clear_stop_reason(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_trace_set_source(ushort ConnectNo, ushort source);
		[DllImport("LTSMC.dll")]
        public static extern short smc_read_trace_data(ushort ConnectNo, ushort axis, int bufsize, double[] time, double[] pos, double[] vel, double[] acc, ref int recv_num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_trace_start(ushort ConnectNo, ushort AxisNum, ushort[] AxisList);
        [DllImport("LTSMC.dll")]
        public static extern short smc_trace_stop(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_enable(ushort ConnectNo, ushort PwmNo, ushort enable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_enable(ushort ConnectNo, ushort PwmNo, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_fix_high_width(ushort ConnectNo, ushort pwm_no, ushort enable, double high_width_s);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_fix_high_width(ushort ConnectNo, ushort pwm_no, ref ushort enable, ref double high_width_s);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_output(ushort ConnectNo, ushort PwmNo, double fDuty, double fFre);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_output(ushort ConnectNo, ushort PwmNo, ref double fDuty, ref double fFre);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_pwm_output(ushort ConnectNo, ushort Crd, ushort pwmno, double fDuty, double fFre);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_follow_speed(ushort ConnectNo, ushort pwmno, ushort mode, double MaxVel, double MaxValue, double OutValue);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_follow_speed(ushort ConnectNo, ushort pwmno, ref ushort mode, ref double MaxVel, ref double MaxValue, ref double OutValue);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_onoff_duty(ushort ConnectNo, ushort PwmNo, double fOnDuty, double fOffDuty);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_onoff_duty(ushort ConnectNo, ushort PwmNo, ref double fOnDuty, ref double fOffDuty);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_follow_onoff(ushort ConnectNo, ushort pwmno, ushort Crd, ushort on_off);
		[DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_follow_onoff(ushort ConnectNo, ushort pwmno, ref ushort Crd, ref ushort on_off);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_pwm_to_start(ushort ConnectNo, ushort Crd, ushort pwmno, ushort on_off, double delay_value, ushort delay_mode, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_pwm_to_stop(ushort ConnectNo, ushort Crd, ushort pwmno, ushort on_off, double delay_time, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_ahead_pwm_to_stop(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double ahead_value, ushort ahead_mode, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_write_pwm(ushort ConnectNo, ushort Crd, ushort pwmno, ushort on_off, double ReverseTime);
		[DllImport("LTSMC.dll")]
        public static extern short smc_laser_set_output(ushort ConnectNo, ushort Enable, ushort Width);
		[DllImport("LTSMC.dll")]
        public static extern short smc_laser_get_output(ushort ConnectNo, ref ushort Enable, ref ushort Width);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_counter_inmode(ushort ConnectNo, ushort axis, ushort mode);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_counter_inmode(ushort ConnectNo, ushort axis, ref ushort mode);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_counter_reverse(ushort ConnectNo, ushort axis, ushort reverse);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_counter_reverse(ushort ConnectNo, ushort axis, ref ushort reverse);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_encoder_unit(ushort ConnectNo, ushort axis, double pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_encoder_unit(ushort ConnectNo, ushort axis, ref double pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_extra_encoder_mode(ushort ConnectNo, ushort channel, ushort inmode, ushort multi);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_extra_encoder_mode(ushort ConnectNo, ushort channel, ref ushort inmode, ref ushort multi);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_extra_encoder(ushort ConnectNo, ushort axis, int pos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_extra_encoder(ushort ConnectNo, ushort axis, ref int pos);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pmove_unit_extern(ushort ConnectNo, ushort axis, double Dist, ushort posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_lookahead_mode(ushort ConnectNo, ushort Crd, ushort enable, int LookaheadSegments, double PathError, double LookaheadAcc);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_lookahead_mode(ushort ConnectNo, ushort Crd, ref ushort enable, ref int LookaheadSegments, ref double PathError, ref double LookaheadAcc);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_arc_limit(ushort ConnectNo, ushort Crd, ushort Enable, double MaxCenAcc, double MaxArcError);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_arc_limit(ushort ConnectNo, ushort Crd, ref ushort Enable, ref double MaxCenAcc, ref double MaxArcError);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_open_list(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_close_list(ushort ConnectNo, ushort Crd);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_stop_list(ushort ConnectNo, ushort Crd, ushort stop_mode);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_pause_list(ushort ConnectNo, ushort Crd);	
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_start_list(ushort ConnectNo, ushort Crd);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_change_speed_ratio(ushort ConnectNo, ushort Crd, double percent);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_run_state(ushort ConnectNo, ushort Crd);
        [DllImport("LTSMC.dll")]
        public static extern int smc_conti_remain_space(ushort ConnectNo, ushort Crd);	
        [DllImport("LTSMC.dll")]
        public static extern int smc_conti_read_current_mark(ushort ConnectNo, ushort Crd);	
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_line_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] pPosList, ushort posi_mode, int mark);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_center_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, int Circle, ushort posi_mode, int mark);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_radius_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double Arc_Radius, ushort Arc_Dir, int Circle, ushort posi_mode, int mark);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_3points_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Mid_Pos, int Circle, ushort posi_mode, int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Cen_Pos, double Angle, double[] Target_Pos, ushort posi_mode, long mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_center_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Angle, ushort Arc_Dir, long Circle, ushort posi_mode, long mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_circle_move_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Cen_Pos, double Angle, double[] Target_Pos, ushort posi_mode, long mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_wait_input(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double TimeOut, int mark);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_outbit_to_start(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double delay_value, ushort delay_mode, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_outbit_to_stop(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double delay_time, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_ahead_outbit_to_stop(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double ahead_value, ushort ahead_mode, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_accurate_outbit_unit(ushort ConnectNo, ushort Crd, ushort cmp_no, ushort on_off, ushort map_axis, double rel_dist, ushort pos_source, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_write_outbit(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double ReverseTime);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_clear_io_action(ushort ConnectNo, ushort Crd, uint Io_Mask);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_pause_output(ushort ConnectNo, ushort Crd, ushort action, int mask, int state);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_pause_output(ushort ConnectNo, ushort Crd, ref ushort action, ref int mask, ref int state);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_override(ushort ConnectNo, ushort Crd, double Percent);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_blend(ushort ConnectNo, ushort Crd, ushort enable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_blend(ushort ConnectNo, ushort Crd, ref ushort enable);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_pmove_unit(ushort ConnectNo, ushort Crd, ushort axis, double dist, ushort posi_mode, ushort mode, int imark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay(ushort ConnectNo, ushort Crd, double delay_time, int mark);
		[DllImport("LTSMC.dll")]
        public static extern short smc_line_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Dist, ushort posi_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_center_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, int Circle, ushort posi_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_radius_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double Arc_Radius, ushort Arc_Dir, int Circle, ushort posi_mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_3points_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Mid_Pos, int Circle, ushort posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Cen_Pos, double Angle, double[] Target_Pos, ushort posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_center_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Angle, ushort Arc_Dir, long Circle, ushort posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_circle_move_center_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, long Circle, ushort posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_profile_unit(ushort ConnectNo, ushort Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_profile_unit(ushort ConnectNo, ushort Crd, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
		[DllImport("LTSMC.dll")]
		public static extern short smc_set_vector_profile_unit_acc(ushort ConnectNo, ushort Crd, double Min_Vel, double Max_Vel, double acc, double dec, double Stop_Vel);
		[DllImport("LTSMC.dll")]
		public static extern short smc_get_vector_profile_unit_acc(ushort ConnectNo, ushort Crd, ref double Min_Vel, ref double Max_Vel, ref double acc, ref double dec, ref double Stop_Vel);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_s_profile(ushort ConnectNo, ushort Crd, ushort s_mode, double s_para);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_s_profile(ushort ConnectNo, ushort Crd, ushort s_mode, ref double s_para);
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_dec_stop_time(ushort ConnectNo, ushort Crd, double time);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_dec_stop_time(ushort ConnectNo, ushort Crd, ref double time);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pvt_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double[] pVel);
        [DllImport("LTSMC.dll")]
        public static extern short smc_pts_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double[] pPercent);
        [DllImport("LTSMC.dll")]
        public static extern short smc_pvts_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double velBegin, double velEnd);
        [DllImport("LTSMC.dll")]
        public static extern short smc_ptt_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos);
        [DllImport("LTSMC.dll")]
        public static extern short smc_pvt_move(ushort ConnectNo, ushort AxisNum, ushort[] AxisList);
		[DllImport("LTSMC.dll")]
        public static extern short smc_cam_table_unit(ushort ConnectNo, ushort MasterAxisNo, ushort SlaveAxisNo, uint Count,  double[] pMasterPos,  double[] pSlavePos, ushort SrcMode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_cam_move(ushort ConnectNo, ushort AxisNo);
        [DllImport("LTSMC.dll")]
        public static extern short smc_cam_move_cycle(ushort ConnectNo, ushort iaxis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_set_mode(ushort ConnectNo, ushort Axis, ushort mode);
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_get_mode(ushort ConnectNo, ushort Axis,ref ushort mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_set_amplitude_tacc(ushort ConnectNo, ushort Axis, double tacc_s);
		[DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_unit(ushort ConnectNo, ushort Axis, double Amplitude, double Frequency);
		[DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_stop(ushort ConnectNo, ushort Axis);
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_set_cycle_num(ushort ConnectNo, ushort Axis, uint cycle_num);
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_get_cycle_num(ushort ConnectNo, ushort Axis, ref uint cycle_num);
		[DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_axislist(ushort ConnectNo, ushort AxisSelIndex, ushort AxisNum, ushort[] AxisList);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_axislist(ushort ConnectNo, ushort AxisSelIndex, ref ushort AxisNum, ushort[] AxisList);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_ratiolist(ushort ConnectNo, ushort AxisSelIndex, ushort StartRatioIndex, ushort RatioSelNum, double[] RatioList);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_ratiolist(ushort ConnectNo, ushort AxisSelIndex, ushort StartRatioIndex, ushort RatioSelNum, double[] RatioList);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_mode(ushort ConnectNo, ushort InMode, ushort IfHardEnable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_mode(ushort ConnectNo, ref ushort InMode, ref ushort IfHardEnable);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_index(ushort ConnectNo, ushort AxisSelIndex, ushort RatioSelIndex);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_index(ushort ConnectNo, ref ushort AxisSelIndex, ref ushort RatioSelIndex);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_move(ushort ConnectNo, ushort ForceMove);
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_stop(ushort ConnectNo);
		[DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_emg_logic(ushort ConnectNo, ushort emg_logic);
		[DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_emg_logic(ushort ConnectNo, ref ushort emg_logic);
		[DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_input(ushort ConnectNo, ref ushort axis_sel_input, ref ushort ratio_sel_input, ref ushort emg_input);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode(UInt16 ConnectNo, UInt16 axis, UInt16 inmode, Int32 multi, double vh);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode(UInt16 ConnectNo, UInt16 axis, ref UInt16 inmode, ref Int32 multi, ref double vh);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode_extern(UInt16 ConnectNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode_extern(UInt16 ConnectNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode_decimals(UInt16 ConnectNo, UInt16 axis, UInt16 inmode, double multi, double vh);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode_decimals(UInt16 ConnectNo, UInt16 axis, ref UInt16 inmode, ref double multi, ref double vh);
		[DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode_extern_decimals(UInt16 ConnectNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, double[] multi);
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode_extern_decimals(UInt16 ConnectNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, double[] multi);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_enter(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_stop(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_clear(ushort ConnectNo, ushort axis);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_run_state(ushort ConnectNo, ushort axis, ref int RunState, ref int Remain, ref int NotRunned, ref int Runned);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_add_data(ushort ConnectNo, ushort axis, int size, int[] data_table);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_start_multi(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_pause_multi(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, ushort [] ResultList);
		[DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_stop_multi(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);
		[DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_pwm_follow_speed(ushort ConnectNo, ushort Crd, ushort pwm_no, ushort mode, double MaxVel, double MaxValue, double OutValue);
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_pwm_follow_speed(ushort ConnectNo, ushort Crd, ushort pwm_no, ref ushort mode, ref double MaxVel, ref double MaxValue, ref double OutValue);
		[DllImport("LTSMC.dll")]
        public static extern short smc_t_pmove_extern_unit(UInt16 ConnectNo, UInt16 axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc_time, double dec_time, UInt16 posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_t_pmove_extern_softstart_unit(UInt16 ConnectNo, UInt16 axis, double MidPos, double TargetPos, double start_Vel, double Max_Vel, double stop_Vel, double delay_time, double Max_Vel2, double stop_vel2, double acc_time, double dec_time, UInt16 posi_mode);
		[DllImport("LTSMC.dll")]
        public static extern short smc_update_target_position_extern_unit(UInt16 ConnectNo, UInt16 axis, double mid_pos, double aim_pos, double vel, UInt16 posi_mode); 
		[DllImport("LTSMC.dll")]
        public static extern short smc_ellipse_move(UInt16 ConnectNo, ushort Crd, UInt16 axisNum, UInt16[] Axis_List, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, UInt16 Dir, UInt16 Pos_Mode);
	}
}
