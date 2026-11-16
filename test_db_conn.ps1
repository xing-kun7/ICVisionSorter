Add-Type -AssemblyName 'System.Data'
$connStr = 'Server=127.0.0.1,1433;Database=VisualSortingDB;User Id=sa;Password=Sql2022@Password;TrustServerCertificate=True;'
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
try {
    $conn.Open()
    Write-Host "Database connection OPENED successfully!"
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT Name, Password, Role FROM Member'
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host ("User: " + $reader["Name"] + " | Password: " + $reader["Password"] + " | Role: " + $reader["Role"])
    }
}
finally {
    $conn.Close()
}
