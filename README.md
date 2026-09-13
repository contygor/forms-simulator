# Forms Simulator — Formulário em Nuvem

[![.NET](https://img.shields.io/badge/.NET%2010-ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-PostgreSQL-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Frontend](https://img.shields.io/badge/Frontend-HTML%2C%20CSS%20e%20JavaScript-E34F26?logo=html5&logoColor=white)](frontend/)
[![Docker](https://img.shields.io/badge/Containers-Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![AWS](https://img.shields.io/badge/Cloud-AWS-FF9900?logo=amazonaws&logoColor=white)](https://aws.amazon.com/)

> Aplicação web simples para cadastro e consulta de dados pessoais, construída com frontend, API e banco de dados em containers, com foco em ser um projeto de fácil implantação na AWS para estudo da plataforma.

O projeto simula um formulário inspirado no Google Forms. O usuário pode cadastrar **nome, sobrenome, data de nascimento e cidade de nascimento** ou consultar se um cadastro com esses dados já existe.

Além de funcionar localmente, o projeto foi organizado para facilitar a migração para uma arquitetura em nuvem utilizando **Amazon ECR, Amazon ECS/Fargate, Application Load Balancer, VPC e Amazon RDS PostgreSQL**.

## Sobre o projeto

O sistema é dividido em três componentes:

```text
										Usuário
											 │
											 ▼
							Frontend em Nginx
											 │ HTTP
											 ▼
							API ASP.NET Core
											 │ Entity Framework Core
											 ▼
								PostgreSQL
```

O frontend possui duas abas:

* **Cadastrar**: envia os dados para `POST /api/forms`.
* **Consultar**: verifica os dados por meio de `POST /api/forms/check`, sem retornar o registro armazenado.

A API também disponibiliza `GET /health`, que pode ser usado por Docker, ECS ou um Application Load Balancer para verificar se a aplicação está ativa.

## Objetivo de nuvem

O intuito do projeto é servir como uma aplicação simples, mas completa, para estudar e praticar uma implantação em nuvem na AWS.

A arquitetura planejada para produção é:

```text
Internet
	 │
	 ▼
Application Load Balancer público
	 │
	 ├── Frontend ECS/Fargate
	 │
	 └── API ECS/Fargate
						│
						▼
			 Amazon RDS PostgreSQL
```

Na AWS, o Load Balancer será o componente público. As tasks do frontend e da API poderão ficar em subnets privadas, e o banco deverá permanecer em subnets privadas sem IP público.

O projeto ainda contém a infraestrutura local em Docker Compose. A criação manual da VPC, ECR, ECS, ALB, RDS, Security Groups, IAM, HTTPS e DNS será realizada posteriormente no console da AWS.

## Estrutura do repositório

```text
forms-simulator/
│
├── Data/
│   └── AppDbContext.cs
│
├── Migrations/
│   ├── 20260913034853_InitialCreate.cs
│   ├── 20260913034853_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
│
├── Models/
│   ├── Form.cs
│   ├── FormCheckRequest.cs
│   └── FormCreateRequest.cs
│
├── frontend/
│   ├── app.js
│   ├── config.js
│   ├── config.template.js
│   ├── Dockerfile
│   ├── index.html
│   ├── nginx.conf
│   └── styles.css
│
├── tests/FormsSimulator.Tests/
│   ├── ApiTests.cs
│   ├── AppDbContextTests.cs
│   └── FormsSimulator.Tests.csproj
│
├── .dockerignore
├── .gitignore
├── docker-compose.yml
├── Dockerfile
├── forms-simulator.csproj
├── forms-simulator.http
├── Program.cs
└── README.md
```

### Diretórios e arquivos principais

| Diretório ou arquivo | Descrição |
| --- | --- |
| `frontend/` | Interface web, configuração do Nginx e imagem do frontend |
| `Models/` | Entidades persistidas e objetos de entrada da API |
| `Data/` | Configuração do Entity Framework Core e acesso ao banco |
| `Migrations/` | Histórico da estrutura do banco PostgreSQL |
| `tests/` | Testes do modelo, persistência e endpoints HTTP |
| `Program.cs` | Inicialização da API e definição das rotas |
| `Dockerfile` | Imagem multi-stage da API ASP.NET Core |
| `docker-compose.yml` | Ambiente local com frontend, API e PostgreSQL |
| `.dockerignore` | Arquivos excluídos do contexto de build Docker |
| `.gitignore` | Arquivos gerados e configurações locais ignorados pelo Git |

## Endpoints da API

### Health check

```http
GET /health
```

Resposta:

```json
{
	"status": "ok"
}
```

### Cadastrar uma resposta

```http
POST /api/forms
Content-Type: application/json
```

Corpo:

```json
{
	"nome": "Ana",
	"sobrenome": "Silva",
	"dataNascimento": "1990-05-20T00:00:00",
	"cidadeNascimento": "Recife"
}
```

### Consultar um cadastro

```http
POST /api/forms/check
Content-Type: application/json
```

Corpo:

```json
{
	"nome": "Ana",
	"sobrenome": "Silva",
	"dataNascimento": "1990-05-20T00:00:00",
	"cidadeNascimento": "Recife"
}
```

Resposta:

```json
{
	"cadastrado": true
}
```

A consulta usa `POST` para que os dados pessoais não sejam enviados pela URL, evitando exposição no histórico do navegador e em logs de requisições.

## Execução local com Docker Compose

### Requisitos

* Docker
* Docker Compose

Suba os três containers:

```bash
docker compose up --build -d
```

Verifique o estado dos serviços:

```bash
docker compose ps
```

Endereços locais:

```text
Frontend: http://localhost:3000
API:      http://localhost:8080
Swagger:  http://localhost:8080/swagger
Health:   http://localhost:8080/health
```

O PostgreSQL é acessível apenas pela rede interna do Compose. A porta `5432` não é publicada no host.

Para acompanhar os logs:

```bash
docker compose logs -f
```

Para parar os containers:

```bash
docker compose down
```

Para parar os containers e remover também o volume local do banco:

```bash
docker compose down -v
```

## Configuração por ambiente

O frontend recebe a URL da API pela variável:

```text
FRONTEND_API_URL=http://localhost:8080
```

A API recebe a conexão do banco pela variável:

```text
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=formdb;Username=postgres;Password=suasenha
```

Em produção, esses valores devem ser substituídos por configurações da AWS. Senhas não devem ser armazenadas no Git; use AWS Secrets Manager ou secrets da task do ECS.

## Testes

Execute os testes automatizados com:

```bash
dotnet test tests/FormsSimulator.Tests/FormsSimulator.Tests.csproj
```

Os testes cobrem:

* Mapeamento relacional do modelo.
* Persistência usando SQLite em memória.
* Endpoint de health check.
* Cadastro de uma resposta.
* Consulta de cadastro existente.
* Consulta de cadastro inexistente.
* Rejeição de data de nascimento futura.

## Próximos passos na AWS

1. Criar repositórios no Amazon ECR para as imagens da API e do frontend.
2. Criar uma VPC com subnets públicas e privadas.
3. Criar um cluster ECS com Fargate.
4. Criar o Amazon RDS PostgreSQL em subnets privadas.
5. Criar Security Groups para ALB, frontend, API e RDS.
6. Criar um Application Load Balancer público.
7. Configurar Target Groups para frontend e API.
8. Armazenar as credenciais do banco no AWS Secrets Manager.
9. Configurar HTTPS pelo AWS Certificate Manager.
10. Publicar as tasks no ECS e validar o health check.

## Licença

Projeto para estudo e prática de desenvolvimento web, containers e arquitetura em nuvem na AWS.

© 2025 Ygor Araujo

[![GitHub](https://img.shields.io/badge/GitHub-contygor-181717?logo=github&logoColor=white)](https://github.com/contygor)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Ygor%20Araujo-0A66C2?logo=linkedin&logoColor=white)](https://www.linkedin.com/in/contygor/)
