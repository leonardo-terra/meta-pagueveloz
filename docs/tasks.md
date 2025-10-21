## Plano de Implementação: PagueVeloz

### Fase 1: Fundação e Estrutura do Projeto

O objetivo desta fase é configurar a arquitetura da solução, definir os modelos de domínio principais e preparar o banco de dados.

* [ ] **Configurar a Arquitetura da Solução (Clean Architecture)**
    * **Critérios de Aceite (AC):**
        * [ ] Criar a estrutura de projetos: `.Domain`, `.Application`, `.Infrastructure`, `.API` (ou `.Web`).
        * [ ] Definir as dependências corretas (ex: `.Domain` não depende de ninguém, `.Application` depende do `.Domain`, `.Infrastructure` e `.API` dependem do `.Application`).
        * [ ] Configurar um container de Injeção de Dependência (DI) nativo do .NET 9.

* [ ] **Definir Modelos de Domínio Principais**
    * **AC:**
        * [ ] Criar a entidade `Client` (Cliente).
        * [ ] Criar a entidade `Account` (Conta) com propriedades: `Balance` (saldo total), `ReservedBalance` (saldo reservado), `CreditLimit` (limite), `Status` (ativo, inativo, etc.).
        * [ ] Adicionar um método `AvailableBalance()` na entidade `Account` que calcula (`Balance - ReservedBalance`).
        * [ ] Criar a entidade `Transaction` (Transação) com `OperationType`, `Amount`, `ReferenceId`, `Timestamp`, `Status`, etc.

* [ ] **Configurar Persistência de Dados (EF Core)**
    * **AC:**
        * [ ] Adicionar o EF Core ao projeto `.Infrastructure`.
        * [ ] Criar o `DbContext` e configurar os `DbSet`s para `Client`, `Account` e `Transaction`.
        * [ ] Implementar a primeira migração (migration) e aplicá-la ao banco de dados (SQL Server ou PostgreSQL).
        * [ ] Configurar o padrão Repositório (Repository) ou Unit of Work.

* [ ] **Criar Endpoints Básicos da API**
    * **AC:**
        * [ ] Criar um endpoint `POST /api/accounts` que aceita o JSON de criação de conta (conforme exemplo).
        * [ ] Criar um endpoint `POST /api/transactions` que aceita o JSON de operação (conforme exemplo).
        * [ ] No momento, esses endpoints podem apenas aceitar a requisição e retornar um `200 OK` ou `201 Created` sem lógica de negócio.

* [ ] **(Diferencial) Dockerizar a Aplicação**
    * **AC:**
        * [ ] Criar um `Dockerfile` para a `.API`.
        * [ ] Criar um `docker-compose.yml` que sobe a API e o banco de dados (SQL Server ou PostgreSQL).
        * [ ] O sistema deve ser totalmente funcional executando `docker-compose up`.

### Fase 2: Validação de Entrada Robusta e Segurança

O objetivo é implementar validação rigorosa de dados de entrada e melhorar a segurança do sistema antes de implementar a lógica de negócio.

* [ ] **Implementar Validação Rigorosa de Dados de Entrada**
    * **AC:**
        * [ ] Criar validators usando FluentValidation para todos os DTOs de entrada.
        * [ ] Validar formato de `account_id` (ex: deve seguir padrão "ACC-XXX" ou UUID).
        * [ ] Validar formato de `reference_id` (ex: deve ser único e seguir padrão "TXN-XXX" ou UUID).
        * [ ] Validar `amount` (deve ser positivo, não zero, dentro de limites razoáveis).
        * [ ] Validar `currency` (deve estar em lista permitida: BRL, USD, EUR).
        * [ ] Implementar sanitização de `metadata` para prevenir injeção de dados maliciosos.
        * [ ] Adicionar validação de tamanho máximo para campos de texto.

* [ ] **Implementar Validação de Contas e Estados**
    * **AC:**
        * [ ] Validar se `account_id` existe antes de processar operações.
        * [ ] Validar se conta está ativa (status != "inactive" ou "blocked").
        * [ ] Validar se cliente da conta está ativo.
        * [ ] Implementar validação de limites de crédito por cliente.

* [ ] **Implementar Logs de Auditoria Detalhados**
    * **AC:**
        * [ ] Log estruturado de todas as operações com timestamp, usuário, IP.
        * [ ] Log de tentativas de operações inválidas ou suspeitas.
        * [ ] Log de mudanças de estado de contas.
        * [ ] Implementar correlação de logs por `reference_id`.

### Fase 3: Lógica de Negócio Principal (Operações Simples)

O objetivo é implementar as operações financeiras mais fundamentais (crédito e débito) e suas validações.

* [ ] **Implementar Criação de Conta**
    * **AC:**
        * [ ] O endpoint `POST /api/accounts` deve salvar corretamente uma nova `Account` no banco de dados, associada a um `Client`.
        * [ ] A conta deve ser criada com `initial_balance` (se fornecido) e `credit_limit`.

* [ ] **Implementar Operação de 'credit'**
    * **AC:**
        * [ ] Uma requisição `credit` deve aumentar o `Balance` da conta especificada.
        * [ ] A transação deve ser registrada no histórico da conta.
        * [ ] O endpoint deve retornar o JSON de saída com `status: "success"` e os saldos atualizados.

* [ ] **Implementar Operação de 'debit' (com Validação)**
    * **AC:**
        * [ ] Uma requisição `debit` deve diminuir o `Balance` da conta.
        * [ ] O sistema deve validar se `(AvailableBalance + CreditLimit) >= Amount`.
        * [ ] Se a validação passar (Caso #1 e #2), a transação é `success` e o saldo é atualizado.
        * [ ] Se a validação falhar (Caso #1 e #2), a transação é `failed`, o saldo *não* é alterado, e uma `error_message` ("Saldo insuficiente" ou "Limite excedido") é retornada.

### Fase 4: Lógica de Negócio Avançada (Reservas e Transferências)

O objetivo é implementar as operações que envolvem múltiplos saldos (reserva/captura) ou múltiplas contas (transferência), exigindo maior controle transacional.

* [ ] **Implementar Operação de 'reserve'**
    * **AC:**
        * [ ] Uma requisição `reserve` deve validar se `AvailableBalance >= Amount`.
        * [ ] Se sucesso, `AvailableBalance` diminui e `ReservedBalance` aumenta no mesmo valor. O `Balance` total *não* muda.
        * [ ] Se falhar, retornar `status: "failed"` (Saldo disponível insuficiente).
        * [ ] Cobre o Caso de Uso #3.

* [ ] **Implementar Operação de 'capture'**
    * **AC:**
        * [ ] Uma requisição `capture` deve validar se `ReservedBalance >= Amount`.
        * [ ] Se sucesso, `ReservedBalance` diminui e `Balance` (total) também diminui. `AvailableBalance` *não* muda.
        * [ ] Se falhar, retornar `status: "failed"` (Saldo reservado insuficiente).
        * [ ] Cobre o Caso de Uso #3.

* [ ] **Implementar Operação de 'transfer' (Atomicidade)**
    * **AC:**
        * [ ] A operação requer uma conta de origem e uma de destino (ex: via `metadata`).
        * [ ] O sistema executa um `debit` na origem e um `credit` no destino.
        * [ ] A operação inteira deve ser atômica (usar `Database.BeginTransactionAsync()`).
        * [ ] Se o débito na origem falhar (ex: saldo insuficiente), o crédito no destino *não* deve ocorrer (rollback).
        * [ ] Cobre o Caso de Uso #4.

* [ ] **Implementar Operação de 'reversal'**
    * **AC:**
        * [ ] A operação deve identificar uma transação anterior (ex: via `metadata.original_reference_id`).
        * [ ] A lógica deve reverter o efeito da transação original (ex: reverter um `debit` aplica um `credit`).
        * [ ] O sistema deve impedir reversões duplicadas.

### Fase 5: Resiliência e Concorrência (Robustez)

Esta é a fase mais crítica, garantindo que o sistema é seguro contra condições de corrida e falhas.

* [ ] **Garantir Atomicidade por Operação (Unit of Work)**
    * **AC:**
        * [ ] Cada chamada ao `POST /api/transactions` deve ser executada dentro de uma transação de banco de dados.
        * [ ] A atualização do saldo da `Account` e a inserção do registro de `Transaction` devem ocorrer juntas (commit) ou falhar juntas (rollback).

* [ ] **Implementar Controle de Concorrência (Lock Pessimista)**
    * **AC:**
        * [ ] Ao processar uma transação, a linha da `Account` correspondente deve ser bloqueada no banco de dados para escrita.
        * [ ] (Usando EF Core: ex: `context.Accounts.Where(a => a.Id == accountId).SetTrackingBehavior(QueryTrackingBehavior.NoTracking).Select(...).ForUpdate()` - a sintaxe exata depende do provider, como Npgsql).
        * [ ] Testes de concorrência (ex: 100 débitos simultâneos em paralelo) devem resultar no saldo final correto, sem corrupção de dados.

* [ ] **Implementar Idempotência (reference_id)**
    * **AC:**
        * [ ] Antes de processar *qualquer* operação, o sistema deve verificar se já existe uma `Transaction` com o mesmo `reference_id`.
        * [ ] Se o `reference_id` existir e a transação original foi `success`, o sistema deve retornar a resposta original *sem* reprocessar a lógica de negócio.
        * [ ] Se o `reference_id` existir e a transação original foi `failed`, o sistema pode optar por re-processar ou retornar a falha original.
        * [ ] O `reference_id` deve ter um índice único (Unique Index) no banco de dados para garantir a proteção em nível de DB.

### Fase 6: Operações Assíncronas e Observabilidade (NFRs)

O objetivo é desacoplar o sistema, adicionar monitoramento e preparar para produção.

* [ ] **Implementar Publicação de Eventos Assíncronos**
    * **AC:**
        * [ ] Após uma transação ser *comitada* com sucesso no banco de dados, um evento (ex: `TransactionProcessedEvent`) é disparado.
        * [ ] Para este desafio, pode ser um "Mediator" (como MediatR) publicando uma notificação `async`. Em um sistema real, seria um RabbitMQ/Kafka.
        * [ ] A API deve responder ao cliente *antes* que o processamento assíncrono do evento termine.

* [ ] **Implementar Retry (Caso #5)**
    * **AC:**
        * [ ] Criar um "handler" para o evento assíncrono (ex: `INotificationHandler` se usar MediatR).
        * [ ] Simular uma falha nesse handler (ex: lançar uma exceção).
        * [ ] Implementar uma política de retry (ex: usando Polly) com backoff exponencial para o *processamento do evento*, garantindo que ele seja re-tentado.

* [ ] **Implementar Observabilidade (Logs e Health Checks)**
    * **AC:**
        * [ ] Adicionar logs estruturados (ex: Serilog) em pontos-chave (início da requisição, erro de validação, sucesso da transação, falha de concorrência).
        * [ ] Adicionar um endpoint `/health` (usando `AspNetCore.HealthChecks`) que verifica a conectividade com o banco de dados.

* [ ] **Implementar Testes Unitários e de Integração**
    * **AC:**
        * [ ] Criar testes unitários para a lógica de negócio pura (ex: validação de saldo na entidade `Account`).
        * [ ] Criar testes de integração (usando `WebApplicationFactory`) que chamam a API real e verificam o estado do banco de dados (em memória ou testcontainer).
        * [ ] Incluir um teste específico para concorrência (conforme Fase 4).

### Fase 7: Testes de Performance e Stress

O objetivo é validar o sistema sob condições de alta carga e concorrência.

* [ ] **Implementar Testes de Carga e Performance**
    * **AC:**
        * [ ] Configurar NBomber ou Artillery para testes de carga.
        * [ ] Criar cenário de teste com 1000+ transações simultâneas na mesma conta.
        * [ ] Criar cenário de teste com 10.000+ transações distribuídas em múltiplas contas.
        * [ ] Medir latência média, percentil 95 e 99 de cada operação.
        * [ ] Verificar que não há corrupção de dados durante testes de concorrência.

* [ ] **Implementar Testes de Volume de Dados**
    * **AC:**
        * [ ] Criar teste com 100.000+ transações históricas no banco.
        * [ ] Medir performance de consultas com grandes volumes de dados.
        * [ ] Testar paginação e filtros em listagens de transações.
        * [ ] Verificar performance de relatórios com agregações.

* [ ] **Implementar Testes de Recursos e Memória**
    * **AC:**
        * [ ] Monitorar uso de memória durante picos de carga.
        * [ ] Verificar ausência de vazamentos de memória.
        * [ ] Testar comportamento do garbage collector sob carga.
        * [ ] Medir uso de CPU e identificar gargalos.

* [ ] **Implementar Testes de Resiliência**
    * **AC:**
        * [ ] Simular falhas temporárias de banco de dados.
        * [ ] Testar comportamento com timeouts de rede.
        * [ ] Verificar recuperação automática após falhas.
        * [ ] Testar circuit breaker e retry policies.

### Fase 8: Tratamento de Edge Cases e Cenários Críticos

O objetivo é garantir que o sistema lida corretamente com cenários extremos e inesperados.

* [ ] **Implementar Tratamento de Edge Cases de Valores**
    * **AC:**
        * [ ] Tratar operações com `amount = 0` (retornar erro apropriado).
        * [ ] Tratar operações com valores negativos (retornar erro de validação).
        * [ ] Tratar valores extremamente grandes (validar limites).
        * [ ] Tratar valores com muitas casas decimais (arredondar ou rejeitar).

* [ ] **Implementar Tratamento de Contas e Referências Inexistentes**
    * **AC:**
        * [ ] Retornar erro claro quando `account_id` não existe.
        * [ ] Retornar erro claro quando `reference_id` já existe (idempotência).
        * [ ] Tratar tentativas de operação em contas inativas/bloqueadas.
        * [ ] Implementar validação de contas de destino em transferências.

* [ ] **Implementar Tratamento de Timeouts e Falhas de Infraestrutura**
    * **AC:**
        * [ ] Configurar timeouts apropriados para operações de banco.
        * [ ] Implementar retry com backoff para falhas temporárias.
        * [ ] Tratar falhas de conexão com banco de dados.
        * [ ] Implementar fallback para operações críticas.

* [ ] **Implementar Tratamento de Cenários de Concorrência Extrema**
    * **AC:**
        * [ ] Testar cenário onde múltiplas operações tentam esgotar o mesmo saldo.
        * [ ] Tratar deadlocks em operações de transferência.
        * [ ] Implementar timeout para locks de longa duração.
        * [ ] Tratar cenário de operações simultâneas de reserve/capture.

* [ ] **Implementar Tratamento de Dados Corrompidos e Inconsistências**
    * **AC:**
        * [ ] Implementar validação de integridade de dados.
        * [ ] Tratar cenários onde saldo calculado não bate com transações.
        * [ ] Implementar processo de reconciliação de dados.
        * [ ] Tratar cenários de transações órfãs ou duplicadas.

* [ ] **Implementar Tratamento de Cenários de Negócio Especiais**
    * **AC:**
        * [ ] Tratar tentativas de reversal de operações já revertidas.
        * [ ] Tratar capture de reservas expiradas ou inexistentes.
        * [ ] Implementar validação de transferências para a mesma conta.
        * [ ] Tratar operações em contas com limite de crédito zerado.

