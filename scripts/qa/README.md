# Scripts de QA offline

Para testes focados em **janelas, ações do usuário, navegação e erros visíveis**, use a [suíte de usabilidade](USABILITY.md): `.\scripts\Invoke-UsabilityQA.ps1`. Ela abre janelas reais com dispositivos simulados e gera capturas e passos por jornada.

A suíte combina **C#** para protocolo, estado da aplicação e componentes WPF, **PowerShell** para execução e relatórios e **Python** para medições independentes de áudio. São 38 casos vinculados a subconjuntos de 39 das 57 baterias de `CubeControl-1.1/Cube/docs/desenvolvimento/qa-test-plan.md`. Essa associação não significa cobertura integral de uma bateria.

## Executar

Na raiz do repositório, em PowerShell:

```powershell
# Lista as 57 baterias e a quantidade de casos implementados; não executa testes.
.\scripts\Invoke-QA.ps1 -List

# Seleção rápida, com compilação e evidências fora do repositório.
.\scripts\Invoke-QA.ps1 -Battery QA-MIDI-001,QA-FILE-001

# Processamento nativo e análise independente dos WAVs exportados.
.\scripts\Invoke-QA.ps1 -Battery QA-DSP-003 -AnalyzeAudio

# Todos os casos offline implementados, incluindo regressões existentes.
.\scripts\Invoke-QA.ps1 -AllOffline -AnalyzeAudio
```

Requisitos: Windows, PowerShell 5.1+, SDK .NET 10 e a toolchain MSVC usada pelo projeto desktop para compilar o DSP nativo. A primeira compilação pode restaurar dependências NuGet. Python 3.9+ é necessário apenas com `-AnalyzeAudio`; não requer pacotes adicionais. Use `-Python caminho\python.exe` se necessário.

Opções adicionais:

| Opção | Comportamento |
| --- | --- |
| `-HostKind Protocol` / `Desktop` | Restringe o executável; o relatório identifica esse escopo. |
| `-IncludeDependencies` | Acrescenta as dependências do índice do plano à seleção. Não certifica pré-requisitos nem impõe execução sequencial entre baterias: os casos usam fixtures independentes. Dependências sem implementação permanecem pendentes. |
| `-NoBuild` | Usa os binários existentes no diretório de saída; execute uma compilação após mudar o código. |
| `-OutputRoot C:\CubeControl-QA` | Define a raiz de compilação e relatórios, obrigatoriamente fora do repositório. |
| `-TimeoutSeconds 900` | Limite por processo de compilação ou testes. Ao exceder, encerra a árvore do processo e mantém as evidências já gravadas. |

Sem seleção, o launcher apenas lista. Para chamar pelo `powershell.exe -File`, uma seleção separada por vírgulas também é aceita como argumento único.

## Evidências e resultados

Cada execução cria `C:\CubeControl-QA\runs\<data-hora-id>`, com:

- `summary.md` e `summary.json`: resultado por subconjunto, códigos dos processos, dependências não selecionadas e escopo.
- `Protocol/results.json` e `Desktop/results.json`: casos individuais, duração e exceções. São atualizados após cada caso; `Running` identifica o caso em andamento se houver interrupção.
- `junit.xml` por executável, para integração com CI.
- Logs de compilação e execução, arquivos de evidência por caso e hashes dos fontes do checkout.
- `audio-analysis.json`, quando solicitado: frequência, duração, RMS, pico, correlação estéreo e verificações por WAV.

Código de saída do launcher: **0** = subconjuntos selecionados passaram; **1** = falha de teste, compilação, processo ou análise de áudio; **2** = seleção pendente/incompleta sem outra falha. Nos logs de processo, 124 indica timeout e 127 indica falha ao iniciar o executável. Um erro de argumentos é rejeitado antes da execução.

`AUTOMATED_SUBSET_PASSED` nunca equivale a aprovação da bateria inteira. Uma análise Python reprovada retorna código 1 mesmo se os casos C# de estrutura e duração passaram; consulte também o resultado de áudio no resumo. Os hashes descrevem o checkout, inclusive mudanças locais; com `-NoBuild`, não comprovam equivalência entre fontes e binários.

## Cobertura e limites

O transporte MIDI é simulado em memória. Os casos exercitam framing, timeout, rejeição de respostas, conflitos, coalescência, backup/recuperação, arquivos de efeitos, AMPs, IRs, preferências e as suítes de regressão de efeitos existentes.

O host WPF usa dispatcher STA e recursos da aplicação, com casos de comandos, edição, histórico, controles, navegação, playlist, waveform, exportação e validação estrutural de firmware. Não mostra a janela principal nem inicializa a descoberta de dispositivos. Isso não substitui testes visuais de DPI, foco, leitor de tela, geometria ou operação humana.

Os testes não abrem portas MIDI, saídas de áudio nem executam o gravador de firmware. Arquivos sintéticos e dados temporários ficam na pasta de evidências. Cenários de hardware, audição, desempenho prolongado e os cenários ainda não implementados continuam no plano manual. `-AllOffline` seleciona somente baterias com casos registrados.

O Python mede uma senoide conhecida de 1 kHz, estéreo em oposição de fase, processada em nove combinações de velocidade e pitch. Verifica frequência com tolerância de 1,5%, quantidade de frames com tolerância de um frame, formato, presença de sinal, pico abaixo de 0,999 e correlação abaixo de -0,95. Esses limites são explícitos e específicos dessa fixture; não constituem avaliação perceptiva de música.

## Organização

- `CubeControl-1.1/Cube/windows/Qa.Shared`: executor, relatórios e conexão MIDI simulada.
- `CubeControl-1.1/Cube/windows/Cube.Tests/QaProtocol.cs`: casos de protocolo.
- `CubeControl-1.1/Cube/windows/Cube.Desktop.Tests/QaDesktop.cs`: casos WPF e áudio.
- `scripts/qa/analyze_audio.py`: análise WAV independente, somente biblioteca padrão.

Os executáveis preservam a execução anterior sem `--qa`. Para inspecionar seus registros diretamente:

```powershell
dotnet C:\CubeControl-QA\build\bin\Cube.Tests\debug\Cube.Tests.dll --qa --list
dotnet C:\CubeControl-QA\build\bin\Cube.Desktop.Tests\debug\Cube.Desktop.Tests.dll --qa --list
```

Ao adicionar um caso, mantenha a declaração `Test("id", "descrição", "QA-XXX-001,...", ...)` em uma linha: o launcher lê esse catálogo. Cada caso recebe uma pasta exclusiva. Use dados sintéticos, verificações de resultado e isolamento; não inicialize serviços de dispositivo no host offline.

## Achados da validação inicial

Na execução completa de 03/10/2026, ambos os projetos compilaram sem erros: **21/22 casos de protocolo**, **16/16 casos desktop** e **7/9 fixtures de áudio** passaram. Evidências locais em `C:\CubeControl-QA\runs\20261003-165705-fe7d2e70`. O launcher também foi verificado com uma seleção aprovada (saída 0) e uma bateria manual sem casos (saída 2).

As verificações mantêm falhas visíveis, sem mudar código de produção para obter aprovação:

- A suíte existente de reverb falha na asserção `external bank edit blocks recovery`, em `ReverbTests.cs`. O relatório guarda a exceção para investigar a divergência entre implementação e contrato do teste.
- Dois WAVs sintéticos exportados a velocidade 1,5× (pitch 0 e +6 semitons) atingem o limite PCM16. Os picos antes da conversão são aproximadamente 1,0306 e 1,0964, respectivamente. A análise de ausência de saturação reprova esses arquivos; frequência, duração e fase passam.

Esses achados precisam de investigação/correção separada. Nenhum arquivo de produção foi alterado pela implementação dos scripts.

