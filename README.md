# RpgRooms

Projeto de exemplo de salas para campanhas de RPG usando Blazor, EF Core e SignalR.

## Ferramentas necessárias

Para aplicar as migrações do Entity Framework Core é necessário ter a ferramenta de linha de comando `dotnet-ef` instalada.

```bash
dotnet tool install --global dotnet-ef
```

Se a ferramenta já estiver instalada, atualize-a:

```bash
dotnet tool update --global dotnet-ef
```

O comando `dotnet ef database update` utilizado na seção de execução depende dessa ferramenta.

## Execução

Certifique-se de ter o .NET SDK 8.0 instalado. O projeto `RpgRooms.Core`
depende do sistema de autorização do ASP.NET Core e inclui a referência ao
pacote `Microsoft.AspNetCore.Authorization` para compilar corretamente.

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
