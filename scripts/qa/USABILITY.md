# Suíte de usabilidade e interface

Esta suíte abre a janela WPF real e executa nove jornadas com controles renderizados, UI Automation e eventos de teclado. Use uma sessão Windows interativa e desbloqueada. Durante a execução, as janelas de teste podem receber foco; evite interagir com elas. Não há conexão com pedal nem saída de áudio físico.

```powershell
# Compila e executa as nove jornadas.
.\scripts\Invoke-UsabilityQA.ps1

# Lista a relação com as baterias do plano.
.\scripts\Invoke-UsabilityQA.ps1 -List

# Seleciona os cenários relacionados à navegação.
.\scripts\Invoke-UsabilityQA.ps1 -Battery QA-NAV-001,QA-NAV-002,QA-NAV-003

# Reutiliza a última compilação, quando os fontes não mudaram.
.\scripts\Invoke-UsabilityQA.ps1 -NoBuild
```

Requisitos: Windows, PowerShell 5.1+, .NET 10 e a toolchain MSVC já usada pelo aplicativo. Python não é necessário. O diretório padrão é `C:\CubeControl-UsabilityQA`, separado da suíte anterior para evitar compartilhar intermediários de compilação WPF.

## Jornadas automatizadas

| Caso | Ações e resultado exigido |
| --- | --- |
| `ux-navigation` | Percorre todos os 25 pares de páginas usando o controle de navegação. A página selecionada deve estar visível, coincidir com o menu e preservar edição pendente. |
| `ux-responsive` | Percorre cinco páginas em 640×480, 920×740 e 1280×800. Os cinco destinos de navegação devem caber na janela. Gera 15 imagens para revisão do restante do conteúdo. |
| `ux-gallery` | Abre a galeria duas vezes seguidas, exige um único diálogo, cancela, troca de página e retorna. Repete três vezes sem modificar o banco simulado. |
| `ux-dirty-close` | Tenta fechar com alteração pendente, verifica o proprietário da confirmação real, clica Cancelar e continua navegando sem perder a edição. |
| `ux-busy-close` | Tenta fechar durante uma operação controlada, exige aviso, fecha o aviso e verifica que a janela continua utilizável após concluir a operação. |
| `ux-error-recovery` | Injeta erro de arquivo na operação, exige mensagem de erro visível e verifica navegação e próxima ação. |
| `ux-keyboard` | Dá foco ao controle de volume, envia seta, sai da página, retorna e salva pelo botão real. Verifica valor e conclusão do salvamento. |
| `ux-effect-windows` | Para Chorus, Phaser, Delay e Reverb: abre, minimiza, reabre, exige reutilização da mesma janela, fecha e repete. Verifica proprietário e ausência de janelas órfãs. O dispositivo simulado não certifica processamento real dos efeitos. |
| `ux-seeded-actions` | Executa 80 ações em ordem reproduzível: navegar, selecionar preset, usar setas no volume e abrir/fechar opções. Exige coerência entre menu e página, ausência de bloqueio permanente e de janelas inesperadas. Seed: `20261003`. |

As interações são feitas no mesmo processo por peers de UI Automation e eventos WPF. Algumas preparações, como conectar o simulador, selecionar um efeito e injetar uma falha, usam o modelo diretamente. Portanto, esta é uma suíte de integração da UI, não uma certificação de mouse físico, teclado global ou automação externa do executável instalado.

## Relatórios e investigação

Validação em 03/10/2026: **9/9 jornadas aprovadas**, com **47 capturas** em `C:\CubeControl-UsabilityQA\runs\20261003-173232-ef098e95`. O teste deliberado do monitor reprovou como esperado, registrando a exceção em `C:\CubeControl-UsabilityQA\monitor-probe-20261003`. Isso confirma os cenários descritos, não uma aprovação geral de usabilidade.

Cada execução cria `C:\CubeControl-UsabilityQA\runs\<data-hora-id>`:

- `summary.md` e `summary.json`: resultado dos subconjuntos selecionados.
- `Desktop/results.json` e `Desktop/junit.xml`: resultado por jornada e stack trace.
- Uma pasta por jornada com `steps.json`, imagens PNG, `bindings.json` e `exceptions.json`.
- `failure.png` quando uma asserção falha; diálogos em janelas separadas também recebem imagem.
- `geometry.json` na jornada de tamanhos e `seed.json` na sequência reproduzível.
- Logs de processo e compilação. Se houver timeout, `results.json` indica o último caso em andamento.

O monitor registra exceções não tratadas do dispatcher e reprova a jornada. Apenas no host de teste, marca essas exceções como tratadas para conseguir salvar evidências e continuar; isso não muda o tratamento de erros do aplicativo normal. Exceções fatais do processo também são registradas quando possível. Tarefas não observadas são registradas caso o runtime as publique, mas o momento dessa publicação não é garantido. Warnings de binding são evidências para revisão, não falhas automáticas.

O teste do próprio monitor é deliberadamente reprovado e não faz parte das nove jornadas:

```powershell
dotnet C:\CubeControl-UsabilityQA\build\bin\Cube.Desktop.Tests\debug\Cube.Desktop.Tests.dll --usability --qa --exception-probe --batteries QA-RES-001 --results C:\CubeControl-UsabilityQA\monitor-probe-novo
```

Use uma pasta de resultados nova. O resultado correto dessa verificação é saída **1**, caso `exception-probe` reprovado e `QA_EXPECTED_DISPATCHER_PROBE` no arquivo `unhandled.log`.

O launcher retorna **0** quando todas as jornadas selecionadas passam, **1** quando há falha e **2** quando a seleção inclui cenários sem implementação. Nenhum resultado automatizado aprova uma bateria inteira do plano.

## Revisão humana complementar

Use as capturas e a aplicação para registrar **passou / falhou / não avaliado**, sempre com passos e evidências. Estes critérios não recebem aprovação automática:

| Área | Pergunta de avaliação |
| --- | --- |
| Sentido da navegação | O nome de cada destino corresponde ao conteúdo? A pessoa entende como voltar à tarefa anterior? |
| Clareza das ações | É possível distinguir aplicar, pré-visualizar, salvar, restaurar e cancelar antes de agir? |
| Erros | A mensagem explica o que falhou, o que foi preservado e qual ação permite continuar? |
| Janelas e diálogos | O diálogo permanece acessível, o conteúdo cabe, o foco retorna e não há janela atrás do proprietário? |
| Teclado | Tab/Shift+Tab, Enter e Escape funcionam em todos os diálogos sem prender o foco? |
| Layout | Há texto truncado, sobreposição, rolagem inacessível ou ação essencial fora da área útil? |
| Ambiente | Repetir com DPI real 125/150/200%, monitores diferentes, temas e idiomas suportados. Redimensionar a janela não equivale a mudar DPI. |
| Estados reais | Repetir conexão/desconexão, arquivos escolhidos pelo diálogo do sistema e falhas reais de dispositivo em ambiente apropriado. |
| Acessibilidade | Validar leitura, ordem e significado dos controles com leitor de tela; conferir contraste e indicação de foco. |

## Alteração para isolamento

O construtor interno de `MainWindow` recebeu `initializeDevices = true`. A suíte passa `false` para impedir `Model.Initialize()` e `BackingTracks.Initialize()` no evento Loaded. O construtor público e os demais chamadores mantêm o padrão original. A janela, XAML, handlers de navegação, diálogos e comandos continuam sendo os do aplicativo.
