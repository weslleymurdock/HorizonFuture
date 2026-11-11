# Atendimento e Serviços Sociais (Social Service Delivery)

## Escopo implementado

O Stage 03 implementa o domínio responsável por representar a operação de atendimento e serviços sociais de uma organização da sociedade civil (ONG/OSC).

Uma instalação do HorizonFuture atende a uma única ONG; os identificadores organizacionais (`OrganizationId`, `OrganizationalUnitId`, `DepartmentId`) definem escopos operacionais e locais de atendimento, sem introduzir mecanismos de multitenancy.

O domínio reutiliza a identidade canônica `HF.Domain.Stakeholders.Person`. Um beneficiário é qualquer pessoa que participa de programas ou recebe atendimentos, sem duplicação de dados civis ou de contato.

A participação em programas sociais e as matrículas/inscrições possuem ciclo de vida próprio e **não se confundem com `OrganizationalMembership`** (vínculo institucional com a ONG).

---

## Glossário do domínio

| Conceito | Tipo de componente | Responsabilidade |
|---|---|---|
| `SocialProgram` | Agregado (`AggregateRoot`) | Iniciativa continuada destinada a determinado público ou finalidade social, com critérios e linhas de atendimento. |
| `OperatingPeriod` | Objeto de Valor (`ValueObject`) | Período de vigência operacional do programa (`StartDate`, `EndDate`). |
| `ServiceOffering` | Entidade (`EntityBase`) | Linha de atendimento ou serviço ofertado no âmbito de um programa. |
| `IEligibilityCriterion` / `EligibilityCriterionBase` | Objeto de Valor (`ValueObject`) | Requisitos verificáveis para ingresso no programa (faixa etária, território, grupo-alvo, documentos, atributos). |
| `ApplicantProfile` | Objeto de Valor (`ValueObject`) | Perfil de dados declarados e comprovados do candidato para avaliação de elegibilidade. |
| `EligibilityAssessmentResult` | Objeto de Valor (`ValueObject`) | Resultado técnico da verificação dos critérios de elegibilidade. |
| `ProgramEnrollment` | Agregado (`AggregateRoot`) | Solicitação formal de ingresso em um programa, avaliação e decisão administrativa. |
| `ProgramParticipation` | Agregado (`AggregateRoot`) | Participação ativa e contínua do beneficiário no programa, com histórico de transições. |
| `ParticipationHistoryEntry` | Objeto de Valor (`ValueObject`) | Registro histórico imutável de transição de estado da participação (`TransitionDate`, `Reason`, `ChangedBy`). |
| `SocialCareEncounter` | Agregado (`AggregateRoot`) | Interação de atendimento individual, familiar ou domiciliar realizada por equipe técnica. |
| `SensitiveCareNotes` | Objeto de Valor (`ValueObject`) | Anotações operacionais sensíveis com finalidade legítima declarada e restrição de acesso. |
| `DeliveredService` | Agregado (`AggregateRoot`) | Registro de serviço executado ou agendado em favor de um beneficiário. |
| `DeliveredBenefit` | Agregado (`AggregateRoot`) | Benefício eventual ou continuado (alimentos, kit escolar, vestuário) concedido e entregue. |
| `BenefitQuantity` | Objeto de Valor (`ValueObject`) | Quantidade positiva e unidade de medida do benefício concedido. |
| `SocialReferral` | Agregado (`AggregateRoot`) | Encaminhamento para a rede socioassistencial externa (CRAS, CREAS, UBS) ou interna. |
| `SocialActivity` | Agregado (`AggregateRoot`) | Atividade regular desenvolvida no escopo de um programa. |
| `ActivitySession` | Agregado (`AggregateRoot`) | Ocorrência concreta (sessão/encontro) com horários, facilitadores e lista de presença. |
| `SessionAttendanceRecord` | Objeto de Valor (`ValueObject`) | Registro de presença ou ausência (com justificativa obrigatória se justificada). |
| `SocialEvidence` | Agregado (`AggregateRoot`) | Metadados e referências abstratas a documentos, termos assinados ou fotos de comprovação. |

---

## Diagrama de relacionamentos

```mermaid
erDiagram
    Person ||--o{ ProgramEnrollment : applies
    Person ||--o{ ProgramParticipation : participates
    Person ||--o{ SocialCareEncounter : attended
    Person ||--o{ DeliveredBenefit : receives
    Person ||--o{ DeliveredService : receives
    Person ||--o{ SessionAttendanceRecord : attends
    Person ||--o{ PersonRelationship : relates

    SocialProgram ||--o{ ServiceOffering : offers
    SocialProgram ||--o{ EligibilityCriterionBase : requires
    SocialProgram ||--o{ ProgramEnrollment : receives
    SocialProgram ||--o{ ProgramParticipation : manages
    SocialProgram ||--o{ SocialActivity : organizes
    SocialProgram ||--o{ DeliveredBenefit : grants
    SocialProgram ||--o{ DeliveredService : delivers

    SocialActivity ||--o{ ActivitySession : schedules
    ActivitySession ||--o{ SessionAttendanceRecord : records
    ActivitySession ||--o{ SessionFacilitator : staffed_by

    ProgramEnrollment ||--o| ProgramParticipation : originates
    ProgramEnrollment ||--o| EligibilityAssessmentResult : assessed_by

    SocialCareEncounter ||--o| SensitiveCareNotes : encapsulates
    SocialCareEncounter ||--o{ SocialReferral : generates
    SocialCareEncounter ||--o{ SocialEvidence : references
```

---

## Limites de agregados e invariantes

### 1. `SocialProgram`
- **Ciclo de vida:** `Draft` -> `Active` <-> `Suspended` -> `Completed` / `Archived`.
- **Inscrições:** Um programa só pode aceitar novas inscrições se seu estado for estritamente `Active`.
- **Suspensão:** Requer motivo não vazio. Não pode suspender programas em `Draft`, `Completed` ou `Archived`.
- **Conclusão:** Data de encerramento não pode ser anterior à data de início das operações.
- **Imutabilidade:** Programas arquivados ou concluídos rejeitam adições de critérios, ofertas ou edições cadastrais.

### 2. `ProgramEnrollment` vs `OrganizationalMembership`
- Inscrição em programa social representa a formalização de demanda e triagem para um serviço específico.
- É independente do vínculo institucional `OrganizationalMembership`. Uma pessoa pode ser membro da ONG sem estar inscrita em programas sociais, ou participar de vários programas sem ser associada ou colaboradora.
- **Ciclo de vida:** `Pending` -> `UnderReview` -> `Approved` / `Waitlisted` / `Rejected` / `Cancelled` / `Withdrawn`.
- **Aprovação e Elegibilidade:** Somente permitida se o programa estiver `Active`. Se houver avaliação de elegibilidade anexada e o resultado for inelegível (`!IsEligible`), a aprovação é estritamente bloqueada (`DomainException("enrollment.ineligible")`). Se o programa contiver critérios obrigatórios e nenhuma avaliação tiver sido anexada, a aprovação também é rejeitada (`DomainException("enrollment.assessment_required")`). Decisões de aprovação rejeitam datas anteriores à inscrição (`enrollment.invalid_decision_date`).
- **Motivo obrigatório e Proteção Terminal:** Rejeição, cancelamento, lista de espera e desistência exigem justificativa e responsável explícitos. Inscrições em estados terminais (`Rejected`, `Cancelled`, `Withdrawn`) não podem ser reabertas por operações indiretas.
- **Proteção contra Adulteração Pós-Participação:** Inscrições que já originaram uma participação ativa não podem sofrer cancelamento administrativo ou desistência (`enrollment.cannot_cancel_with_active_participation`, `enrollment.cannot_withdraw_with_active_participation`).
- **Participação única:** Inscrições aprovadas originam uma `ProgramParticipation` via `CreateParticipation()`. O agregado registra `HasParticipation` e `ParticipationId`, impedindo categoricamente a geração de participações duplicadas para a mesma inscrição (`DomainException("enrollment.participation_already_created")`).

### 3. `ProgramParticipation`
- Representa o vínculo fático e contínuo do beneficiário no programa.
- **Construção protegida e Integridade de Agregado:** O construtor é restrito (`internal`) e recebe obrigatoriamente a instância de `ProgramEnrollment`. Ele valida status `Approved`, cronologia (`startDate >= enrollment.EnrollmentDate`), não nulidade do autor, e invoca atomicamente `enrollment.RegisterParticipation(Id)`. Qualquer tentativa de contornar `CreateParticipation()` instanciando diretamente o construtor falha se a inscrição já possuir participação ou não estiver aprovada. Se a validação falhar, a inscrição não registra vínculo inconsistente.
- **Ciclo de vida:** `Active` <-> `Suspended` -> `Completed` / `Terminated`.
- **Atomicidade e Integridade de Transição:** Todas as validações (argumentos, não nulidade/vazio de responsáveis e justificativas, regras cronológicas e transições de estado permitidas) e a criação da entrada histórica (`ParticipationHistoryEntry`) são executadas **antes** de qualquer mutação de estado ou atualização de datas no agregado. Se qualquer validação falhar, o agregado permanece intacto sem mutações parciais.
- **Histórico:** Cada mudança de estado registra uma entrada imutável em `History` (`ParticipationHistoryEntry`) com data, motivo e responsável.
- **Cronologia:** Nenhuma transição pode ter data anterior ao início da participação nem anterior à última transição registrada.

### 4. Avaliação de Elegibilidade (`EligibilityAssessmentResult` e Critérios)
- **Consistência estrita:** O objeto de valor `EligibilityAssessmentResult` valida que o indicador sintético `IsEligible` seja estritamente coerente com o atendimento de todos os critérios obrigatórios (`isEligible == mandatoryCriteriaMet`). Se houver discrepância, é lançada `DomainException("eligibility.contradictory_result")`.
- **Critérios tipados:** Cada critério avaliado (`CriterionEvaluationResult`) expõe explicitamente se é obrigatório (`IsMandatory`), suportando critérios classificatórios versus eliminatórios sem ambiguidade.
- **Imutabilidade pós-aprovação:** Inscrições já aprovadas não aceitam substituição ou adição posterior de parecer de elegibilidade (`enrollment.assessment_not_allowed`).

### 5. `SocialCareEncounter` (Atendimentos)
- Atendimento individual, familiar, visita domiciliar ou triagem.
- **Não é prontuário médico:** Não registra diagnósticos clínicos (CID), prescrições de remédios ou laudos médicos.
- **Notas sensíveis (`SensitiveCareNotes`):** Exigem finalidade operacional legítima declarada e classificação de confidencialidade.
- **Conclusão e Cancelamento:** A data/hora de atendimento não pode ser futura. Resumo operacional é obrigatório. Cancelamentos e não comparecimentos (`NoShow`) rastreiam explicitamente o responsável pelo registro (`CancelledByPersonId`, `NoShowRecordedByPersonId`). Atendimentos cancelados não admitem inclusão de evidências.
- **Validação de Enum:** Tipo de atendimento (`EncounterType`) é estritamente validado contra valores indefinidos.

### 6. `DeliveredBenefit` e `DeliveredService`
- Diferenciação rigorosa:
  - Serviço agendado/solicitado vs serviço prestado.
  - Benefício concedido (`Granted`) vs benefício despachado (`Dispatched`) vs entregue (`Delivered`).
- **Quantidade do Benefício:** O objeto de valor `BenefitQuantity` exige valor estritamente positivo (`> 0`) e unidade de medida.
- **Rastreabilidade e Auditoria de Benefícios:**
  - `BenefitType`: Validação estrita via `Enum.IsDefined`.
  - `DispatchedDate` e `DispatchedByPersonId`: Registro de despacho com validação de status (`Granted`).
  - `DeliveredDate`, `DeliveredByPersonId` e `ReceivedByPersonId`: Entrega requer data válida (não anterior à concessão ou despacho).
  - `RevokedDate` e `RevokedByPersonId`: Revogação permitida apenas antes da entrega e não anterior à concessão ou despacho.
  - `ReturnedDate` e `ReturnedByPersonId`: Devolução permitida apenas para benefícios já entregues, com data cronológica coerente e motivo explícito.
- **Rastreabilidade e Ciclo de Serviços (`DeliveredService`):**
  - O construtor padrão aceita apenas estados iniciais `Scheduled` ou `Requested`, rejeitando `Completed`, `Cancelled` ou enums indefinidos (`service_delivery.invalid_initial_status`, `service_delivery.invalid_status`).
  - Para registros retroativos de serviços já executados, utiliza-se a fábrica estática explícita `DeliveredService.RecordCompleted(...)`.
  - Conclusão (`MarkAsCompleted`) requer status de origem `Scheduled` ou `Requested` e data não anterior ao agendamento.
  - Cancelamento (`Cancel`) requer autoria obrigatória (`CancelledByPersonId`), justificativa e status de origem `Scheduled` ou `Requested`.
- **Rastreabilidade contábil futura:** Preserva propriedades `ProjectId` e `FundingSourceId` para vinculação nos Stages 04 e 05.

### 7. `SocialReferral` (Encaminhamentos)
- Encaminhamento interno ou externo (CRAS, CREAS, Conselho Tutelar, Posto de Saúde, Defensoria).
- **Distinção de serviço:** Um encaminhamento externo **não é um serviço executado pela ONG**. O ciclo só atinge `Completed` quando a instituição de destino confirmar o acolhimento/resultado.
- **Autoria Obrigatória em Todas as Operações:**
  - `Dispatch(DateOnly dispatchedDate, string dispatchedByPersonId)`
  - `AcknowledgeReceipt(DateOnly acknowledgedDate, string acknowledgedBy)`
  - `RecordOutcome(DateOnly outcomeDate, string outcomeNotes, bool isSuccessful, string recordedByPersonId)`
  - `Cancel(DateOnly cancellationDate, string reason, string cancelledBy)`
- **Regras Cronológicas:** Despacho, ciência, desfecho e cancelamento não podem anteceder a data do encaminhamento. Se houver despacho, a ciência e o desfecho não podem antecedê-lo. Se houver ciência, o desfecho não pode antecedê-la.
- **Proteção Terminal:** Encaminhamentos concluídos (`Completed`) ou cancelados (`Cancelled`) não admitem transições subsequentes.

### 8. `SocialActivity` e `ActivitySession`
- Atividades agrupam encontros e sessões.
- **Cancelamento de Atividade:** Exige responsável obrigatório (`CancelledByPersonId`) e justificativa.
- **Ciclo de Vida e Integridade Temporal da Sessão (`ActivitySession`):**
  - Estados: `Scheduled` -> `InProgress` -> `Completed`, ou `Rescheduled`, ou `Cancelled`.
  - Início (`Start`): Requer status `Scheduled` ou `Rescheduled`. O início efetivo (`ActualStart`) não pode anteceder o início agendado (`ScheduledStart`). Sessões já iniciadas ou canceladas não podem ser iniciadas.
  - Conclusão (`Complete`): Exige estritamente que a sessão esteja `InProgress`. O término efetivo (`ActualEnd`) não pode anteceder `ActualStart`.
  - Registro Retroativo: Sessões realizadas no passado que necessitem ser inseridas já finalizadas devem utilizar a fábrica explícita e auditável `ActivitySession.RecordCompleted(...)`.
  - Reagendamento (`Reschedule`): Exige responsável obrigatório (`RescheduledByPersonId`), justificativa e horários coerentes (`newEnd >= newStart`). Bloqueado se a sessão já estiver `InProgress`, `Completed` ou `Cancelled`.
- **Frequência e Blindagem de Sessão Concluída/Cancelada:**
  - Estados: `Present`, `Absent`, `JustifiedAbsence`, `Excused`, `NotRecorded`.
  - Registros com `NotRecorded` ou valores fora do enum são estritamente rejeitados (`DomainException("attendance.invalid_status")`).
  - Invariante de Justificativa: Ausência justificada (`JustifiedAbsence`) exige obrigatoriamente motivo/justificativa.
  - Sessão Concluída (`Completed`) ou Cancelada (`Cancelled`): Rejeita qualquer inclusão ou alteração de frequência (`session.completed`, `session.cancelled`), selando o histórico contra adulterações posteriores.
  - Duplicidade: Rejeita registro de presença duplicado para a mesma pessoa na mesma sessão. Em caso de falha, os registros existentes são integralmente preservados.

### 9. `SocialEvidence`
- Armazena exclusivamente **metadados e URIs lógicas**.
- Nenhuma entidade de domínio armazena arrays de bytes de imagens ou arquivos.
- Classificação de sensibilidade: `Standard`, `Confidential`, `Restricted`.

---

## Máquinas de estado

### Ciclo de vida do programa social (`SocialProgram`)

```mermaid
stateDiagram-v2
    [*] --> Draft : Criação
    Draft --> Active : Activate()
    Draft --> Archived : Archive()
    Active --> Suspended : Suspend(reason)
    Suspended --> Active : Activate()
    Active --> Completed : Complete(date)
    Suspended --> Completed : Complete(date)
    Suspended --> Archived : Archive()
    Completed --> Archived : Archive()
    Archived --> [*]
```

### Ciclo de vida da inscrição (`ProgramEnrollment`)

```mermaid
stateDiagram-v2
    [*] --> Pending : Inscrição submetida
    Pending --> UnderReview : StartReview()
    Pending --> Waitlisted : Waitlist(reason)
    Pending --> Approved : Approve()
    Pending --> Rejected : Reject(reason)
    Pending --> Cancelled : Cancel(reason)
    Pending --> Withdrawn : Withdraw(reason)

    UnderReview --> Approved : Approve()
    UnderReview --> Waitlisted : Waitlist(reason)
    UnderReview --> Rejected : Reject(reason)
    UnderReview --> Cancelled : Cancel(reason)
    UnderReview --> Withdrawn : Withdraw(reason)

    Waitlisted --> Approved : Approve()
    Waitlisted --> Rejected : Reject(reason)
    Waitlisted --> Cancelled : Cancel(reason)
    Waitlisted --> Withdrawn : Withdraw(reason)

    Approved --> Withdrawn : Withdraw(reason)
    Approved --> [*] : Origina ProgramParticipation
    Rejected --> [*]
    Cancelled --> [*]
    Withdrawn --> [*]
```

### Ciclo de vida da participação (`ProgramParticipation`)

```mermaid
stateDiagram-v2
    [*] --> Active : Start(date)
    Active --> Suspended : Suspend(date, reason)
    Suspended --> Active : Resume(date)
    Active --> Completed : Complete(date, notes)
    Active --> Terminated : Terminate(date, reason)
    Suspended --> Terminated : Terminate(date, reason)
    Completed --> [*]
    Terminated --> [*]
```

### Ciclo de vida do benefício entregue (`DeliveredBenefit`)

```mermaid
stateDiagram-v2
    [*] --> Granted : Concessão
    Granted --> Dispatched : Dispatch(date)
    Granted --> Delivered : MarkAsDelivered(date, deliveredBy, receivedBy)
    Dispatched --> Delivered : MarkAsDelivered(date, deliveredBy, receivedBy)
    Granted --> Revoked : Revoke(reason)
    Dispatched --> Revoked : Revoke(reason)
    Delivered --> Returned : Return(date, reason)
    Delivered --> [*]
    Revoked --> [*]
    Returned --> [*]
```

---

## Privacidade e proteção de dados (Privacy-by-Design)

1. **Minimização de dados:**
   - O domínio não armazena dados de saúde, religião, biometria ou opiniões políticas em campos genéricos.
   - O atendimento social foca nas necessidades operacionais e de vulnerabilidade socioassistencial.
2. **Segregação de notas sensíveis:**
   - `SensitiveCareNotes` é um objeto de valor encapsulado que exige finalidade operacional declarada para existir.
   - Não é exposto em relatórios operacionais gerais ou dashboards públicos.
3. **Parentesco e Acesso:**
   - A existência de um vínculo familiar (`PersonRelationship`) não presume autorização tácita para acesso a dados pessoais ou retiradas de benefícios sem autorização explícita ou guarda legal comprovada (`PersonRelationshipType.LegalGuardian`).
4. **Isolamento de mídia:**
   - Fotos e comprovantes residirão em storage seguro fora do banco relacional, rastreados por `SocialEvidence` com classificação de confidencialidade.
5. **Dados sintéticos:**
   - Todos os testes unitários e documentação usam dados fictícios gerados artificialmente.

---

## Hipóteses e pontos para validação externa posterior

As regras a seguir foram implementadas como hipóteses operacionais sólidas e configuráveis, com validação empírica prevista junto à ONG piloto após a primeira release estável:
1. **Regra de lista de espera:** Inscrições em lista de espera podem ser promovidas diretamente para aprovação assim que uma vaga for liberada pela equipe social.
2. **Entrega de benefícios a representantes:** Benefícios destinados a menores de idade podem ser entregues a guardiões legais registrados via `PersonRelationship`.
3. **Ausência justificada:** Atestados médicos ou declarações escolares comprovam ausência justificada na sessão de atividades e não penalizam a frequência do participante.
4. **Encaminhamentos externos:** O encerramento de um encaminhamento externo depende de retorno formal da entidade de destino (ex: CRAS/CREAS), permanecendo `Dispatched` ou `Acknowledged` até confirmação.
