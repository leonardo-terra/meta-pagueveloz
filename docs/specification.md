# Desafio Técnico PagueVeloz: Sistema de Processamento de Transações Financeiras

## Contexto da Empresa

A **PagueVeloz** é uma empresa de tecnologia voltada para o setor financeiro, especializada em soluções de meios de pagamento, serviços bancários integrados e adquirência.
Nosso propósito é facilitar a vida financeira de empresas e empreendedores por meio de produtos robustos, ágeis e seguros.

Com uma arquitetura orientada a **microsserviços** e foco em **escalabilidade**, operamos em alto volume de transações e buscamos talentos que compartilhem nossa paixão por desempenho, arquitetura limpa e excelência técnica.

---

## Objetivo do Desafio

Implementar um sistema de **processamento de transações financeiras** que será o núcleo transacional de uma nova plataforma de adquirência.
O sistema deve lidar com operações financeiras em um ambiente onde **volume, concorrência e confiabilidade** são críticos.

---

## Como o Sistema Deve Funcionar

### Entrada

O sistema receberá comandos de operações financeiras via **CLI** ou **API REST** em formato **JSON**.

**Campos da operação:**

| Campo          | Significado                                                                       |
| -------------- | --------------------------------------------------------------------------------- |
| `operation`    | Tipo de operação: `credit`, `debit`, `reserve`, `capture`, `reversal`, `transfer` |
| `account_id`   | Identificador único da conta                                                      |
| `amount`       | Valor da operação em centavos (inteiro)                                           |
| `currency`     | Moeda da operação (ex: `"BRL"`)                                                   |
| `reference_id` | Identificador único da transação para idempotência                                |
| `metadata`     | Dados adicionais opcionais                                                        |

**Exemplo de entrada:**

```json
{
  "operation": "credit",
  "account_id": "ACC-001",
  "amount": 10000,
  "currency": "BRL",
  "reference_id": "TXN-001",
  "metadata": {
    "description": "Depósito inicial"
  }
}
```

---

### Saída

Cada operação deve retornar um objeto **JSON** com o resultado da transação:

| Campo               | Significado                                        |
| ------------------- | -------------------------------------------------- |
| `transaction_id`    | Identificador único da transação processada        |
| `status`            | Status da operação: `success`, `failed`, `pending` |
| `balance`           | Saldo atual da conta após a operação               |
| `reserved_balance`  | Saldo reservado da conta                           |
| `available_balance` | Saldo disponível para uso                          |
| `timestamp`         | Data e hora da operação                            |
| `error_message`     | Mensagem de erro (se aplicável)                    |

**Exemplo de saída:**

```json
{
  "transaction_id": "TXN-001-PROCESSED",
  "status": "success",
  "balance": 10000,
  "reserved_balance": 0,
  "available_balance": 10000,
  "timestamp": "2025-07-07T20:05:00Z",
  "error_message": null
}
```

---

## Regras de Negócio

### Contas e Clientes

* **Múltiplas contas por cliente:** Cada cliente pode possuir N contas.
* **Cada conta possui:**

  * Saldo disponível
  * Saldo reservado
  * Limite de crédito
  * Status (active, inactive, blocked)
  * Histórico de transações

**Exemplo: Cliente com conta nova realizando operações simples**

| Operação | Valor      | Saldo Resultante | Status  | Explicação         |
| -------- | ---------- | ---------------- | ------- | ------------------ |
| credit   | R$ 1000,00 | R$ 1000,00       | success | Depósito inicial   |
| debit    | R$ 200,00  | R$ 800,00        | success | Débito aprovado    |
| debit    | R$ 900,00  | R$ 800,00        | failed  | Saldo insuficiente |

---

### Tipos de Operações

* **credit**: adiciona valor ao saldo da conta
* **debit**: remove valor do saldo da conta
* **reserve**: move valor do saldo disponível para o saldo reservado
* **capture**: confirma uma reserva, removendo do saldo reservado
* **reversal**: reverte uma operação anterior
* **transfer**: move valor entre duas contas

---

### Validações de Negócio

* Operações não podem deixar o saldo disponível negativo
* Limite de crédito deve ser respeitado
* Débito considera saldo disponível + limite de crédito
* Reservas só podem ser feitas com saldo disponível
* Capturas só podem ser feitas com saldo reservado suficiente

---

### Controle de Concorrência

* Operações concorrentes na mesma conta devem ser bloqueadas
* Implementar locks otimistas ou pessimistas
* Transações devem ser atômicas

---

### Resiliência e Eventos

* Cada operação deve gerar **eventos assíncronos**
* Implementar **retry com backoff exponencial**
* Garantir **idempotência** com `reference_id`
* Suporte a **rollback** em caso de falhas

---

## Exemplos de Casos de Uso

### Caso #1 – Operações Básicas de Crédito e Débito

Entrada:

```json
[
  {"operation": "credit", "account_id": "ACC-001", "amount": 100000, "currency": "BRL", "reference_id": "TXN-001"},
  {"operation": "debit", "account_id": "ACC-001", "amount": 20000, "currency": "BRL", "reference_id": "TXN-002"},
  {"operation": "debit", "account_id": "ACC-001", "amount": 90000, "currency": "BRL", "reference_id": "TXN-003"}
]
```

Saída:

```json
[
  {"transaction_id": "TXN-001-PROCESSED", "status": "success", "balance": 100000, "available_balance": 100000},
  {"transaction_id": "TXN-002-PROCESSED", "status": "success", "balance": 80000, "available_balance": 80000},
  {"transaction_id": "TXN-003-PROCESSED", "status": "failed", "balance": 80000, "available_balance": 80000}
]
```

---

### Caso #2 – Operações com Limite de Crédito

| Operação | Valor     | Saldo      | Limite    | Status  | Explicação             |
| -------- | --------- | ---------- | --------- | ------- | ---------------------- |
| credit   | R$ 300,00 | R$ 300,00  | R$ 500,00 | success | Depósito inicial       |
| debit    | R$ 600,00 | -R$ 300,00 | R$ 500,00 | success | Usou limite de crédito |
| debit    | R$ 300,00 | -R$ 300,00 | R$ 500,00 | failed  | Excedeu limite         |

---

### Caso #3 – Reserva e Captura

| Operação | Valor      | Saldo Disponível | Saldo Reservado | Status  | Explicação             |
| -------- | ---------- | ---------------- | --------------- | ------- | ---------------------- |
| credit   | R$ 1000,00 | R$ 1000,00       | R$ 0,00         | success | Depósito inicial       |
| reserve  | R$ 300,00  | R$ 700,00        | R$ 300,00       | success | Reserva para pagamento |
| capture  | R$ 300,00  | R$ 700,00        | R$ 0,00         | success | Captura da reserva     |

---

### Caso #4 – Transferência Entre Contas

| Conta Origem | Conta Destino | Valor     | Status  | Explicação             |
| ------------ | ------------- | --------- | ------- | ---------------------- |
| ACC-001      | ACC-002       | R$ 500,00 | success | Transferência aprovada |

---

### Caso #5 – Operações com Falha e Retry

| Operação | Tentativa | Status  | Explicação                    |
| -------- | --------- | ------- | ----------------------------- |
| credit   | 1         | failed  | Falha na publicação do evento |
| credit   | 2         | failed  | Retry com backoff             |
| credit   | 3         | success | Sucesso após retry            |

---

## Requisitos Técnicos

### Linguagem e Framework

* **C# .NET 9** obrigatório
* Uso eficiente de **async/await**
* Aplicação dos princípios **SOLID** e **OOP**

### Arquitetura

* **Clean Architecture**
* Código preparado para divisão futura em microsserviços
* Separação clara de responsabilidades
* Inversão de dependências

### Persistência

* Modelagem relacional adequada
* Suporte a transações distribuídas
* Controle eficiente de concorrência

### Resiliência

* Retry com backoff exponencial
* Fallback strategies
* Circuit breaker pattern
* Idempotência garantida

### Processamento Assíncrono

* Publicação de eventos assíncronos
* Processamento em background
* Consistência eventual

### Observabilidade

* Logs estruturados
* Métricas de performance
* Rastreamento de transações
* Health checks

---

## Critérios de Avaliação

### Qualidades Valorizadas

* **Simplicidade**: fácil entendimento
* **Elegância**: estrutura bem organizada
* **Operacional**: completo e extensível

### Aspectos Técnicos Avaliados

* Transparência referencial
* Qualidade dos testes
* Documentação adequada
* Tratamento de concorrência
* Modelagem de domínio
* Cobertura de testes

### Diferenciais

* Uso de **Docker**
* Métricas de performance
* Observabilidade e eventos de negócio
* Deploy em nuvem
* Padrões avançados: **CQRS**, **Event Sourcing**

---

## Instruções de Execução

### Pré-requisitos

* .NET 9 SDK
* SQL Server ou PostgreSQL (ou banco em memória)
* Docker (opcional)

### Comandos

```bash
# Compilar o projeto
dotnet build

# Executar os testes
dotnet test

# Executar a aplicação
dotnet run --project src/PagueVeloz.TransactionProcessor

# Executar via Docker
docker-compose up
```

---

### Exemplo de Uso da API

```bash
# Criar uma conta
curl -X POST http://localhost:5000/api/accounts \
  -H "Content-Type: application/json" \
  -d '{"client_id": "CLI-001", "initial_balance": 0, "credit_limit": 50000}'

# Realizar um crédito
curl -X POST http://localhost:5000/api/transactions \
  -H "Content-Type: application/json" \
  -d '{"operation": "credit", "account_id": "ACC-001", "amount": 100000, "currency": "BRL"}'
```

---

## Considerações Importantes

### Controle de Concorrência

* Implementar locks adequados
* Considerar **deadlocks** e **timeouts**
* Otimizar para alta concorrência

### Tratamento de Erros

* Não assumir sucesso sempre
* Logging detalhado
* Retornar códigos de erro apropriados

### Segurança

* Validação rigorosa de entrada
* Prevenção contra injeção
* Auditoria completa

### Performance

* Otimização de consultas
* Uso eficiente de memória
* Processamento assíncrono

---

## Importante: Remoção de Informações Pessoais

Antes de enviar a solução:

* Remover dados pessoais de código, comentários e metadados
* Para anonimizar o repositório:

```bash
git archive --format=zip --output=./pagueveloz-challenge.zip HEAD
```

---

**Boa sorte com o desafio!**
