## Plano de Implementação: PagueVeloz

### Fase 1: Fundação e Estrutura do Projeto

O objetivo desta fase é configurar a arquitetura da solução, definir os modelos de domínio principais e preparar o banco de dados.

* [x] **Configurar a Arquitetura da Solução (Clean Architecture)**
    * **Critérios de Aceite (AC):**
        * [x] Criar a estrutura de projetos: `.Domain`, `.Application`, `.Infrastructure`, `.API` (ou `.Web`).
        * [x] Definir as dependências corretas (ex: `.Domain` não depende de ninguém, `.Application` depende do `.Domain`, `.Infrastructure` e `.API` dependem do `.Application`).
        * [x] Configurar um container de Injeção de Dependência (DI) nativo do .NET 9.

* [x] **Definir Modelos de Domínio Principais**
    * **AC:**
        * [x] Criar a entidade `Client` (Cliente).
        * [x] Criar a entidade `Account` (Conta) com propriedades: `Balance` (saldo total), `ReservedBalance` (saldo reservado), `CreditLimit` (limite), `Status` (ativo, inativo, etc.).
        * [x] Adicionar um método `AvailableBalance()` na entidade `Account` que calcula (`Balance - ReservedBalance`).
        * [x] Criar a entidade `Transaction` (Transação) com `OperationType`, `Amount`, `ReferenceId`, `Timestamp`, `Status`, etc.

* [x] **Configurar Persistência de Dados (EF Core)**
    * **AC:**
        * [x] Adicionar o EF Core ao projeto `.Infrastructure`.
        * [x] Criar o `DbContext` e configurar os `DbSet`s para `Client`, `Account` e `Transaction`.
        * [x] Implementar a primeira migração (migration) e aplicá-la ao banco de dados (SQL Server ou PostgreSQL).
        * [x] Configurar o padrão Repositório (Repository) ou Unit of Work.

* [x] **Criar Endpoints Básicos da API**
    * **AC:**
        * [x] Criar um endpoint `POST /api/accounts` que aceita o JSON de criação de conta (conforme exemplo).
        * [x] Criar um endpoint `POST /api/transactions` que aceita o JSON de operação (conforme exemplo).
        * [x] No momento, esses endpoints podem apenas aceitar a requisição e retornar um `200 OK` ou `201 Created` sem lógica de negócio.

* [x] **(Diferencial) Dockerizar a Aplicação**
    * **AC:**
        * [x] Criar um `Dockerfile` para a `.API`.
        * [x] Criar um `docker-compose.yml` que sobe a API e o banco de dados (SQL Server ou PostgreSQL).
        * [x] O sistema deve ser totalmente funcional executando `docker-compose up`.

**✅ FASE 1 CONCLUÍDA** - *Data: 21/10/2025*
- ✅ Arquitetura Clean Architecture implementada
- ✅ Entidades de domínio criadas com GUIDs
- ✅ EF Core configurado com migrações
- ✅ API REST funcional com Swagger
- ✅ Dockerização completa
- ✅ Testes de integração realizados com sucesso

### Fase 2: Validação de Entrada Robusta e Segurança

O objetivo é implementar validação rigorosa de dados de entrada e melhorar a segurança do sistema antes de implementar a lógica de negócio.

* [x] **Implementar Validação Rigorosa de Dados de Entrada**
    * **AC:**
        * [x] Criar validators usando FluentValidation para todos os DTOs de entrada.
        * [x] Validar formato de `account_id` (ex: deve seguir padrão "ACC-XXX" ou UUID).
        * [x] Validar formato de `reference_id` (ex: deve ser único e seguir padrão "TXN-XXX" ou UUID).
        * [x] Validar `amount` (deve ser positivo, não zero, dentro de limites razoáveis).
        * [x] Validar `currency` (deve estar em lista permitida: BRL, USD, EUR).
        * [x] Implementar sanitização de `metadata` para prevenir injeção de dados maliciosos.
        * [x] Adicionar validação de tamanho máximo para campos de texto.

* [x] **Implementar Validação de Contas e Estados**
    * **AC:**
        * [x] Validar se `account_id` existe antes de processar operações.
        * [x] Validar se conta está ativa (status != "inactive" ou "blocked").
        * [x] Validar se cliente da conta está ativo.
        * [x] Implementar validação de limites de crédito por cliente.

* [x] **Implementar Logs de Auditoria Detalhados**
    * **AC:**
        * [x] Log estruturado de todas as operações com timestamp, usuário, IP.
        * [x] Log de tentativas de operações inválidas ou suspeitas.
        * [x] Log de mudanças de estado de contas.
        * [x] Implementar correlação de logs por `reference_id`.

**✅ FASE 2 CONCLUÍDA** - *Data: 21/10/2025*
- ✅ FluentValidation implementado com validators robustos
- ✅ Validação de dados de entrada com sanitização
- ✅ Validação de contas e estados com regras de negócio
- ✅ Sistema de auditoria completo com logs estruturados
- ✅ Validações testadas e funcionando corretamente
- ✅ Segurança aprimorada com validação de IP e User-Agent

### Fase 3: Lógica de Negócio Principal (Operações Simples)

O objetivo é implementar as operações financeiras mais fundamentais (crédito e débito) e suas validações.

* [x] **Implementar Criação de Conta**
    * **AC:**
        * [x] O endpoint `POST /api/accounts` deve salvar corretamente uma nova `Account` no banco de dados, associada a um `Client`.
        * [x] A conta deve ser criada com `initial_balance` (se fornecido) e `credit_limit`.

* [x] **Implementar Operação de 'credit'**
    * **AC:**
        * [x] Uma requisição `credit` deve aumentar o `Balance` da conta especificada.
        * [x] A transação deve ser registrada no histórico da conta.
        * [x] O endpoint deve retornar o JSON de saída com `status: "success"` e os saldos atualizados.

* [x] **Implementar Operação de 'debit' (com Validação)**
    * **AC:**
        * [x] Uma requisição `debit` deve diminuir o `Balance` da conta.
        * [x] O sistema deve validar se `(AvailableBalance + CreditLimit) >= Amount`.
        * [x] Se a validação passar (Caso #1 e #2), a transação é `success` e o saldo é atualizado.
        * [x] Se a validação falhar (Caso #1 e #2), a transação é `failed`, o saldo *não* é alterado, e uma `error_message` ("Saldo insuficiente" ou "Limite excedido") é retornada.

**✅ FASE 3 CONCLUÍDA** - *Data: 21/10/2025*
- ✅ Lógica de criação de conta com validações de negócio robustas
- ✅ Operação de crédito implementada com atualização de saldo
- ✅ Operação de débito implementada com validação de saldo e limite
- ✅ Operações de reserva, captura, reversão e transferência implementadas
- ✅ Processamento de transações com tratamento de erros
- ✅ Regras de negócio aplicadas em todas as operações financeiras
- ✅ Sistema de idempotência para transações duplicadas

### Fase 4: Lógica de Negócio Avançada (Reservas e Transferências)

O objetivo é implementar as operações que envolvem múltiplos saldos (reserva/captura) ou múltiplas contas (transferência), exigindo maior controle transacional.

* [x] **Implementar Operação de 'reserve'**
    * **AC:**
        * [x] Uma requisição `reserve` deve validar se `AvailableBalance >= Amount`.
        * [x] Se sucesso, `AvailableBalance` diminui e `ReservedBalance` aumenta no mesmo valor. O `Balance` total *não* muda.
        * [x] Se falhar, retornar `status: "failed"` (Saldo disponível insuficiente).
        * [x] Cobre o Caso de Uso #3.

* [x] **Implementar Operação de 'capture'**
    * **AC:**
        * [x] Uma requisição `capture` deve validar se `ReservedBalance >= Amount`.
        * [x] Se sucesso, `ReservedBalance` diminui e `Balance` (total) também diminui. `AvailableBalance` *não* muda.
        * [x] Se falhar, retornar `status: "failed"` (Saldo reservado insuficiente).
        * [x] Cobre o Caso de Uso #3.

* [x] **Implementar Operação de 'transfer' (Atomicidade)**
    * **AC:**
        * [x] A operação requer uma conta de origem e uma de destino (ex: via `metadata`).
        * [x] O sistema executa um `debit` na origem e um `credit` no destino.
        * [x] A operação inteira deve ser atômica (usar `Database.BeginTransactionAsync()`).
        * [x] Se o débito na origem falhar (ex: saldo insuficiente), o crédito no destino *não* deve ocorrer (rollback).
        * [x] Cobre o Caso de Uso #4.

* [x] **Implementar Operação de 'reversal'**
    * **AC:**
        * [x] A operação deve identificar uma transação anterior (ex: via `metadata.original_reference_id`).
        * [x] A lógica deve reverter o efeito da transação original (ex: reverter um `debit` aplica um `credit`).
        * [x] O sistema deve impedir reversões duplicadas.

**✅ FASE 4 CONCLUÍDA** - *Data: 21/10/2025*
- ✅ Operação de reserve implementada com validação de saldo disponível
- ✅ Operação de capture implementada com validação de valor reservado
- ✅ Operação de transfer implementada com atomicidade completa
- ✅ Operação de reversal implementada com validação de transação original
- ✅ Segurança transacional aplicada a todas as operações
- ✅ Gerenciamento avançado de saldos (AvailableBalance, ReservedBalance, Balance)
- ✅ Validações robustas para operações complexas
- ✅ Prevenção de reversões duplicadas

### Fase 5: Resiliência e Concorrência (Robustez)

Esta é a fase mais crítica, garantindo que o sistema é seguro contra condições de corrida e falhas.

* [x] **Garantir Atomicidade por Operação (Unit of Work)**
    * **AC:**
        * [x] Cada chamada ao `POST /api/transactions` deve ser executada dentro de uma transação de banco de dados.
        * [x] A atualização do saldo da `Account` e a inserção do registro de `Transaction` devem ocorrer juntas (commit) ou falhar juntas (rollback).

* [x] **Implementar Controle de Concorrência (Lock Pessimista)**
    * **AC:**
        * [x] Ao processar uma transação, a linha da `Account` correspondente deve ser bloqueada no banco de dados para escrita.
        * [x] Implementado usando SQL raw com `FOR UPDATE` para bancos reais e fallback para InMemory em testes.
        * [x] Testes de concorrência (100 débitos simultâneos em paralelo) resultam no saldo final correto, sem corrupção de dados.

* [x] **Implementar Idempotência (reference_id)**
    * **AC:**
        * [x] Antes de processar *qualquer* operação, o sistema deve verificar se já existe uma `Transaction` com o mesmo `reference_id`.
        * [x] Se o `reference_id` existir e a transação original foi `success`, o sistema deve retornar a resposta original *sem* reprocessar a lógica de negócio.
        * [x] Se o `reference_id` existir e a transação original foi `failed`, o sistema pode optar por re-processar ou retornar a falha original.
        * [x] O `reference_id` deve ter um índice único (Unique Index) no banco de dados para garantir a proteção em nível de DB.

**✅ FASE 5 CONCLUÍDA** - *Data: 24/10/2025*
- ✅ Atomicidade completa implementada com transações de banco de dados
- ✅ Controle de concorrência com lock pessimista usando FOR UPDATE
- ✅ Idempotência robusta com verificação de reference_id dentro de transação
- ✅ Testes de concorrência abrangentes validando locks e integridade de dados
- ✅ Suporte a bancos em memória para testes com fallbacks apropriados
- ✅ 5 testes de concorrência passando com 100+ transações simultâneas

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

