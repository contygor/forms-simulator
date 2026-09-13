# forms-simulator

API ASP.NET Core para cadastro e verificacao de respostas de um formulario simples.

## Endpoints

- `GET /health`: verifica se a API esta ativa.
- `POST /api/forms`: cadastra nome, sobrenome, data e cidade de nascimento.
- `POST /api/forms/check`: verifica se os dados informados ja estao cadastrados.
- `/swagger`: documentacao interativa em ambiente de desenvolvimento.

## Execucao local com .NET

Com o PostgreSQL acessivel em `localhost:5432`, execute:

```bash
dotnet ef database update
dotnet run --launch-profile http
```

Swagger: `http://localhost:5065/swagger`

## Execucao com Docker Compose

O Compose inicia a API e o PostgreSQL. O banco fica somente na rede interna do Compose e nao publica a porta `5432` no host.

```bash
docker compose up --build -d
docker compose ps
```

API: `http://localhost:8080`

Swagger: `http://localhost:8080/swagger`

Para parar os containers:

```bash
docker compose down
```

Para parar e apagar tambem os dados locais do PostgreSQL:

```bash
docker compose down -v
```

O Compose define `Database__ApplyMigrations=true`, portanto a API aplica as migrations na inicializacao. As origens permitidas pelo CORS sao configuradas por `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1` e assim por diante.

As credenciais presentes no Compose sao somente para desenvolvimento local. Em producao, use secrets do ambiente de execucao e o endpoint privado do RDS PostgreSQL.

## Testes

```bash
dotnet test tests/FormsSimulator.Tests/FormsSimulator.Tests.csproj
```
