# RpgRooms

Projeto de exemplo de salas para campanhas de RPG usando Blazor, EF Core e SignalR.

## Execução

1. Restaure os pacotes:
   ```bash
   dotnet restore
   ```
2. Aplique as migrações (SQLite por padrão):
   ```bash
   dotnet ef database update --project RpgRooms.Infrastructure --startup-project RpgRooms.Web
   ```
   Para usar SQL Server, defina `DatabaseProvider=SqlServer` e configure a connection string `SqlServerConnection`.
3. Execute a aplicação:
   ```bash
   dotnet run --project RpgRooms.Web
   ```

## Credenciais de desenvolvimento

- Usuário: `admin`
- Senha: `admin`

## Testes

Os testes de regras de negócio podem ser executados com:
```bash
dotnet test
```
