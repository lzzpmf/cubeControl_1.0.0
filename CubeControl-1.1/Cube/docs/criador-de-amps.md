# Criador de amps

[Documentação](README.md) · [Guia de uso](guia-de-uso.md) · Revisão: 02/10/2026

O criador edita dados internos de um modelo Cube Baby em um arquivo `.bin` de **4096 bytes**. Esse formato não é WAV de gabinete, banco JSON A/B/C ou modelo NAM.

## Fluxo disponível

1. Escolha um amp base na interface. A ação confirma a substituição dos ajustes locais, lê o setor do slot no pedal e tenta iniciar a prévia.
2. Como alternativa, importe um `.bin` válido. A importação pela **interface** também tenta aplicar a prévia quando o pedal está disponível; offline, o conteúdo fica no computador.
3. Ajuste os controles e alterne os EQs entre Clássico e Moderno. Mudanças disparam prévia automática; alterações rápidas pendentes são reunidas para enviar o estado mais recente.
4. Exporte um `.bin` para guardar sua edição local.
5. Use restaurar para recuperar a RAM capturada no início da prévia, ou salvar no pedal para substituir explicitamente o setor do slot indicado na confirmação.

Escolher a origem na interface não confirma qual amp está ativo fisicamente. A prévia modifica uma estrutura RAM compartilhada do amp ativo; não existem nove slots independentes de prévia. Trocar o amp no pedal pode recarregar sua RAM.

## Controles atuais

| Controle | Implementação |
| --- | --- |
| Ganho | Multiplica os oito floats da tabela interna; é diferente do GAIN 0–7 do preset |
| Volume do criador | Multiplica a tabela de compensação; é diferente do VOLUME 0–127 do preset |
| EQ A e EQ B finais | Frequência 20–20000 Hz, nível −12 a +12 dB, Q 0,1–10; cinco coeficientes por filtro |
| Coeficientes A e B | Oito grupos por conjunto de 50 floats, com nomes musicais derivados de relatos |
| Resposta por trechos | Dezesseis grupos de 16 valores, de 0–200%; valores resultantes limitados a 0–1 |
| Dados internos do ViewModel | Referência, valores editados e valores a enviar; as tabelas brutas não estão expostas na interface atual |

A02 (Corpo), A04 (Médios) e B01–B03 estão limitados a **90–100%**. Os outros grupos A/B permitem **90–110%**. B02/B03 apresentam três casas decimais e os controles do criador permitem ajuste fino com Shift. Essas faixas são restrições da interface, não garantias de estabilidade acústica ou limites comprovados do DSP.

O centro 100% é relativo à referência carregada. Ajustes sucessivos do fader recalculam a partir dessa referência. A alteração direta de um elemento no ViewModel recaptura a referência do grupo inteiro e devolve o fader a 100%; esse caminho não está exposto como tabela editável no XAML atual. O mapeamento completo está em [Controles e escuta](../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS_CONTROLES_E_ESCUTA.md).

O gráfico de EQ representa os coeficientes calculados, não uma medição de resposta total do pedal. Os conjuntos de 50 coeficientes A/B são distintos dos dois EQs finais. Índices da curva não são bandas de frequência.

## Apresentação e edição

Ganho e Volume ficam no cabeçalho, com referência **1×**. Volume multiplica a compensação do modelo; sua posição final na cadeia DSP não foi comprovada. As quatro categorias preservam os mesmos 40 controles. A EQ usa coral para A, azul e linha tracejada para B e linha neutra para a combinação. Em janelas amplas, os controles da banda ficam ao lado do gráfico; em janelas menores, abaixo. Clássico/Moderno muda apenas a apresentação.

Clique no valor ou pressione Enter para digitar com precisão completa. Enter confirma; Escape ou sair do campo descarta. Valores inválidos não são aplicados. A exibição arredondada nunca retorna automaticamente ao modelo. Mouse, roda, teclado e Shift conservam os passos anteriores.

O ponto junto ao valor identifica diferença da referência do controle. **Restaurar banda** recupera os três parâmetros e os coeficientes originais daquele EQ. Os demais resets mantêm o alcance individual existente. **Sem alterações locais** compara os bytes da edição com o arquivo carregado; não significa original de fábrica ou salvo no pedal.

Os códigos A01–A08/B01–B08 acompanham os nomes de escuta, sem atribuir funções DSP ainda desconhecidas. **Resposta** apresenta Trechos 01–16, cada qual multiplicando 16 elementos contíguos. Não há eixo de intensidade da palhetada, interpolação ou curva de transferência demonstrada. A organização compacta não altera essa operação.

O estado da edição local é separado do resultado operacional. Importar identifica a origem como arquivo; exportar guarda somente o arquivo. A prévia informa a verificação da RAM. **Salvo no slot N** aparece após gravação e comparação integral; nova edição, carregamento, troca de slot ou perda do contexto do dispositivo invalida essa indicação. Não comprova recarga física do som ou retenção após desligamento.

**Restaurar som anterior** recupera a RAM capturada, conservando a edição local. Uma edição posterior pode iniciar outra prévia. A ação fica disponível apenas quando existe RAM anterior capturada e o pedal pode operar. O destino de **Salvar no pedal** permanece visível e é confirmado antes de substituir o slot.

No modo compacto, somente o workspace rola; no modo amplo, a categoria ativa rola entre cabeçalho e ações. Labels, valores e foco permanecem acessíveis sem reduzir fontes.

## RAM, gravação e recuperação

| Operação | Alcance |
| --- | --- |
| Prévia | Corpo de 1568 bytes em região 4, `0x1C–0x63B`; grava apenas trechos diferentes em blocos de até 64 bytes e compara todo o corpo |
| Backup da prévia | `AmpRecovery/ram-<data-hora>.bin`; cabeçalho reconstruído e corpo anterior, sem representar uma leitura de setor flash completo |
| Salvar no pedal | Setor de 4096 bytes em região 0, `0x60000 + slot × 0x1000`, slot interno 0–8 |
| Backup do slot | `AmpRecovery/amp-<posição>-<data-hora>.bin`, lido novamente do arquivo antes de apagar |
| Programação flash | 64 blocos alinhados de 64 bytes, seguida de comparação integral |

A gravação relê o destino, confirma que corresponde ao esperado e preserva o conteúdo anterior. Após apagar, o fluxo não oferece cancelamento voluntário entre blocos. ACK isolado não comprova sucesso. Depois de gravar, selecione outro amp no pedal e volte para recarregar; a prévia RAM é independente da flash.

**Recuperação de amp e de IR são diferentes.** O backup de amp fica no disco, mas a identidade e o estado de prévia de amp ficam em memória no processo. Não há carregamento automático de uma prévia de amp pendente após reiniciar o aplicativo. Use restaurar explicitamente antes de fechar: o fechamento normal chama o encerramento de IR, sem chamar `RestoreAmpCore`; a desconexão manual tenta restaurar ambos. Essa diferença permanece como pendência de implementação.

## Limitações verificadas

- `AmpImage.Validate` aceita ganho e compensação internos de 0 a 100. A antiga restrição de ganho 60 não descreve o código atual; os nove modelos originais possuem verificações de carregamento e preservação dos bytes.
- A validação verifica finitude dos blocos conhecidos, faixas de ganho/compensação/curva e estabilidade dos dois EQs finais. Não prova estabilidade dos conjuntos de 50 coeficientes.
- `SetEq` altera os cinco coeficientes de cada filtro, preservando os metadados vizinhos de frequência/ganho/Q.
- A simulação desabilita leitura, prévia e gravação de amps no dispositivo virtual; importação/exportação local continua disponível.
- O histórico confirma ensaios no aparelho descrito. Não demonstra compatibilidade com qualquer firmware nem equivalência sonora aos amplificadores comerciais usados como referência visual.

## Fontes de implementação

[AmpViewModel](../windows/Cube.Desktop/ViewModels/AmpViewModel.cs), [ações de amp](../windows/Cube.Desktop/ViewModels/EditorViewModel.Amps.cs), [eventos da interface](../windows/Cube.Desktop/Views/AmpEditor.xaml.cs), [formato e validação](../windows/Cube.Protocol/AmpImage.cs), [sessão de amps](../windows/Cube.Protocol/CubeSession.Amps.cs) e [tabelas originais](../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS_FABRICA.md).
