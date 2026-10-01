# Cube Baby — reverb e controles independentes

[Índice](README.md) · [Phaser](CUBE_BABY_PHASER.md) · [Chorus](CUBE_BABY_CHORUS.md) · Investigação de 01/10/2026

## Resultado e alcance

O controle original 0–15 não modifica apenas mistura: ele combina seis coeficientes de realimentação e o ganho do ramo reverberado. Foram identificados também ganho direto, nível de retorno das seis linhas e coeficientes de amortecimento independentes. O editor experimental separa essas cinco famílias de ajuste.

Esta pesquisa não tem guitarra, gravação nem avaliação sonora. Código, snapshots USB e ensaios de RAM sustentam o mapeamento; o modelo matemático produz sinais sintéticos. Nenhum deles certifica qualidade musical, ausência de estalos, headroom ou comportamento acústico prolongado. O controle original passou a chamar-se **Quantidade**, em vez de **Mistura**.

## Fontes, perfil e cadeia de processamento

A pesquisa reutiliza a janela de código do aparelho preservada no phaser, os padrões de captura/restauração do chorus e a distinção entre memória e escuta estabelecida nos amps. A imagem oficial de 2022 é apenas uma referência histórica: seu layout não foi transportado por delta. Nesta investigação, os caminhos foram conferidos diretamente no pedal conectado.

O perfil `cube-baby-reverb-live-8347f8-v1` exige as verificações existentes da base RAM e mais doze assinaturas específicas: aplicação de presets, chamador DSP, worker de reverb, difusão, amortecimento, inicialização, helper dos filtros, bloco inicial, três tabelas e filtro de entrada. A identidade textual do modelo não substitui essas verificações e não identifica um número de série único.

| Elemento | Endereço CPU | Papel |
| --- | --- | --- |
| Base do seletor RAM 04 | `0x8347F8` | Conversão entre endereço CPU e offset USB |
| Aplicação do preset | `0x831AC0` | Campo `03` do preset; seção reverb/cabinet no campo `0A` |
| Ramo de reverb do preset | `0x831E62` | Copia defaults ao ativar; carrega tabelas quando muda 0–15 |
| Chamador DSP | `0x833724` | Modulação → delay → reverb → cabinet |
| Estrutura de reverb | `0x834ED8` | RAM 04 `0x6E0`, 132 bytes |
| Worker | `0x833978` | Rede de seis linhas, difusão, filtros e soma de saída |
| Inicialização | `0xC13D00` | Aloca as linhas no pool e limpa estados |
| Amortecimento inteiro | `0x8338E0` | Filtro por linha com estado externo |
| Difusão | `0x833904` | Dois estágios passa-tudo, coeficiente constante próximo de 0,7 |
| Três biquads | `0x80A8BC` | Coeficientes com stride de 28 bytes; quatro estados por estágio |
| Filtro/decimador de entrada | `0x8359A0` | Produz uma amostra para cada par da entrada |
| Defaults | `0xC48C98` | Imagem inicial da estrutura |
| Tabela de realimentação | `0xC4881C` | 15 conjuntos de seis int16 Q14 |
| Tabela de ganhos | `0xC48D38` | 15 pares float32: direto/reverberado |
| Tabela de byte auxiliar | `0xC488D0` | Valores 8, 16, …, 120; uso musical não demonstrado |

A rotina percorre `quantidade_de_amostras / 2` passos, aplica os atrasos nessa taxa e reconstrói pares de saída. Admitindo a taxa de entrada de 48 kHz sustentada estruturalmente nas pesquisas anteriores, a rede de reverb opera a **24 kHz estimados**. Não foi medida a taxa por áudio nesta sessão.

## Mapa e decisões de exposição

Todos os offsets são relativos ao seletor 04. Os campos float32 e int16 usam little-endian.

| Offset | Tipo e valor original | Uso e decisão |
| --- | --- | --- |
| `0x6E0` | byte, 0 desligado / 3 ativo | Máquina de estados; não editável |
| `0x6E1` | int8, 80 | Multiplica a contribuição das seis linhas por `valor / 128`; editor permite 0–80 |
| `0x6E2` | byte, `8 × quantidade` | Muda pela tabela, mas não é consumido pelo worker/inicializador identificados; preservar |
| `0x6E3–0x6E8` | seis int8, 40/42/44/46/48/50 | Coeficientes de amortecimento por linha; editor usa 11 conjuntos proporcionais, de zero ao original |
| `0x6E9` | byte, zero | Padding preservado |
| `0x6EA–0x6F5` | seis int16 Q14 | Realimentação; editor seleciona os 15 conjuntos originais, independentemente dos ganhos |
| `0x6F6–0x701` | seis int16 | Comprimentos 1193/1259/1319/1381/1439/1511; preservar |
| `0x702–0x705` | dois int16 | Comprimentos de difusão 449/653; preservar |
| `0x706` | int16, 238 | Comprimento do pré-atraso; preservar |
| `0x708` | float32, 1 | Ganho direto independente, editor 0–1 |
| `0x70C` | float32, 0,20–0,90 | Ganho reverberado, editor 0–0,90 |
| `0x710–0x763` | três blocos de sete floats | Cinco coeficientes por biquad e dois floats não usados pelo helper; preservar |

O sinal de saída tem a forma `direto × entrada + reverberado × ramo_processado`; no primeiro elemento de cada par, o ramo processado é interpolado com a amostra anterior. Portanto, os ganhos não formam uma mistura complementar `dry + wet = 1`. Direto zero isola o ramo processado; reverberado zero conserva apenas o sinal direto.

O controle **Retorno das linhas** não equivale ao ganho reverberado: a excitação também entra na soma antes da difusão e dos biquads. Zerar esse retorno não elimina todo o ramo processado. O original 80 representa multiplicador `80/128 = 0,625` por linha.

A realimentação seleciona vetores completos já existentes, em vez de inventar um tempo em segundos. Os extremos vão de `[2943,2676,2454,2245,2065,1862]/16384` a `[14612,14519,14436,14350,14271,14172]/16384`. O maior coeficiente é aproximadamente 0,89185. Os quinze conjuntos e os ganhos correspondentes estão no manifesto de evidências.

No amortecimento, o helper usa `a/512` para o estado anterior e `(512−a)/512` para a amostra atual, com aritmética inteira e saturação. O worker chama esse helper mutável até três vezes por atualização de linha, por causa das verificações de saturação. O modelo conserva essa particularidade; não substitui o caminho por um único filtro ideal. O editor apresenta **0–10; 10 = original**, sem unidade de frequência nem promessa de uma faixa tonal específica.

## Buffers, filtros e candidatos não expostos

O pool começa em CPU `0x818354` e tem `0x5800` bytes. Os defaults consomem 21088 dos 22528 bytes: seis linhas int16, alinhamento, duas linhas float32 e pré-atraso int16. Índices e ponteiros ficam fora da estrutura de controles. Alterar comprimentos durante processamento exigiria reavaliar alocação, índices e inicialização; nenhum deles é escrito nesta versão.

Pré-atraso e comprimentos de difusão existem como campos RAM, mas não foram habilitados como controles de produto. O coeficiente de difusão próximo de 0,7 é literal do código, sem campo RAM independente encontrado. Os três biquads são candidatos concretos a controles tonais futuros, porém não receberam coeficientes novos nesta investigação. Não foram encontrados seletores comprovados de hall/plate/spring ou estéreo neste caminho.

O byte em `0x6E2` não foi chamado de amortecimento, tamanho ou algoritmo: estar em uma tabela e aceitar escrita não prova que altere o DSP. A ausência de leitura vale para os caminhos analisados, não para toda implementação possível de firmware.

## Ensaios USB e restauração

O início físico observado foi PRESET A (`00 01`), reverb zero, modulação desligada e seção reverb/cabinet ligada. As duas capturas de mapeamento tiveram DSP idêntico. Cada série acrescenta três snapshots ociosos antes de escrever. Os diagnósticos exigem aplicativos fechados, portas MIDI únicas e uma pasta nova por execução.

- **Tabela:** valores 0–15, três snapshots por condição, retorno a zero e restauração da estrutura inativa entre valores.
- **Controles:** base 8, duas rodadas de direto 0/0,5; reverberado 0/0,275; realimentação 1/15; retorno 0/40; amortecimento 0/5. Cada ensaio muda uma família de parâmetro.
- **Permanência:** captura imediata, espera de 300 ms, captura, reenvio de VOLUME sem mudança e do mesmo valor 8, captura, mudança para 9, retorno a 8 e restauração.
- **Transições:** personalização dos cinco controles, desligar/religar reverb e desligar/religar a seção reverb/cabinet. A troca física A/B/C permanece como roteiro assistido posterior.

Reenviar a mesma quantidade preserva a personalização. Mudar entre quantidades ativas recarrega realimentação, ganho direto, ganho reverberado e byte auxiliar; retorno e amortecimento permanecem personalizados. Desligar/religar reinicializa a estrutura inteira, perdendo os controles extras.

Somente o float derivado de volume global em `0xF6C–0xF6F` é excluído da comparação estável, com diferenças reportadas explicitamente. Nenhuma máscara de fase do phaser/chorus é reutilizada. A prévia exige modulação desligada para não aceitar estados dinâmicos de outro algoritmo sem um perfil específico. Estados de áudio do reverb vivem fora dos 3952 bytes DSP comparados; não são reescritos para forçar igualdade.

Cada escrita tem intenção preservada, região, endereço, valor anterior e valor pretendido. O verificador cruza esses registros com o tráfego USB bruto e as leituras anteriores, verifica snapshots/banco/contexto, destinos permitidos e restauração. ACK sozinho não confirma alteração. Em desconexão ou contexto desconhecido, os envios param e os backups permanecem; isso não equivale a restauração confirmada.

As séries concluídas preservaram **191 snapshots e 229 escritas**: tabela 53/93, controles 126/115 e transições 12/21. Todas as escritas foram RAM 04 ou parâmetros live 05; nenhuma foi flash. Ao final de **cada uma das três séries**, banco de 48 bytes, estado físico e os **3952 bytes DSP** foram exatamente iguais à referência inicial, sem diferenças nem mesmo no volume derivado. Isso não inclui os buffers/estados transitórios externos à região DSP.

## Editor e recuperação

Na Pedaleira, abra **Reverb → Criador de reverb · experimental**. Ative reverb e seção reverb/cabinet, desligue a modulação, selecione no aplicativo o mesmo preset físico e use **Ler reverb ativo**. A interface oferece sinal direto, sinal reverberado, realimentação, retorno das linhas e amortecimento.

As escritas são serializadas, ajustes rápidos são reunidos e outras prévias/edições de preset ficam bloqueadas. Antes/depois de alterações, o editor confere o DSP protegido, banco e preset físico; o monitor ocioso verifica contexto e reverb. Recarga externa interrompe a prévia, sem reaplicar silenciosamente os controles.

O estado inicial e a intenção ficam em `%LOCALAPPDATA%/CubeControl/pending-reverb-preview.json`, com cópia inicial em `ReverbRecovery/`. **Restaurar reverb**, desconexão normal, releitura do banco e fechamento normal tentam restaurar os campos. Um reinício carrega a recuperação pendente sem reaplicar ajustes. A restauração aceita somente o perfil/contexto correspondente e bytes compatíveis com a transação preservada, inclusive uma escrita interrompida no meio de um vetor. Backup inválido é preservado e bloqueia novas operações.

Arquivos `.reverb.json` usam `cubecontrol-reverb-v1`, com identidade, mapeamento e cinco parâmetros validados. Importar apenas prepara a edição local; **Aplicar ajustes** envia à prévia ativa. Não há endereços arbitrários, salvamento em flash, persistência após desligar nem prévia de reverb no dispositivo simulado.

## Reprodução e testes

Artefatos em [artefatos/reverb](artefatos/reverb/): capturas USB, manifesto `evidence.json`, desmontagem comentada e `model.json`. O manifesto contém hashes de cada arquivo e do ZIP, contagens e resultados de restauração por série.

O [ZIP das capturas](artefatos/reverb/usb-20261001.zip) tem SHA-256 `aaf922bc91cdee493b2f42eb6659de7655af0f76bab8140ac92116693f2ef818`. A [imagem do editor](artefatos/reverb/reverb-editor.png) foi renderizada e inspecionada. O arquivo `input-coefficients.bin` da captura de mapeamento contém estados do decimador, apesar do nome histórico; suas constantes imutáveis são os literais da rotina `input-filter.bin`.

```powershell
python scripts/reverb-evidence.py engenharia-reversa/cube-baby/artefatos/reverb/usb-20261001.zip --output C:/CubeControl-Build/reverb-recheck.json
python scripts/reverb-model.py engenharia-reversa/cube-baby/artefatos/reverb/usb-20261001.zip --output C:/CubeControl-Build/reverb-model-recheck.json
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Tests -- --reverb-only
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -- --reverb-only
```

O verificador e o modelo usam somente a biblioteca padrão Python. A opção `--disassembly CAMINHO` do verificador requer `capstone==5.0.9`; ela omite pools de literais dos trechos desmontados e anota constantes PC-relative. Nenhum script Python se conecta ao pedal.

Passaram **28 verificações de protocolo/recuperação e 11 de integração desktop** do reverb, incluindo firmware específico incompatível, ACK sem alteração, timeout, resposta com endereço divergente, troca física entre escritas, coeficiente parcialmente escrito, arquivo inválido, backup corrompido, importação local e reinício. As regressões de phaser e chorus também passaram: 18 + 9 + 28 verificações de protocolo/pesquisa/prévia e 9 + 14 de desktop. Cópias adulteradas de um snapshot e de um pacote USB foram rejeitadas pelo verificador. O fluxo único de build gerou e verificou o pacote normal em `C:\CubeControl-Reverb-Build\publish\CubeControl-1.1.0`, sem abrir a aplicação nem reconectar ao pedal. Foi usada a opção `-OutputRoot` do mesmo script porque o Windows negou a substituição da pasta no destino padrão; a instalação anterior foi preservada.

O modelo reconstrói decimação, pré-atraso, seis linhas Q15, realimentação Q14, amortecimento, difusão, três biquads e interpolação. Verifica orçamento do pool, coeficientes de realimentação abaixo de um, polos dos biquads dentro do círculo unitário, saída finita, silêncio e identidade de ganho reverberado zero. Onze cenários usam impulso, seno e silêncio. Não reproduz arredondamentos float32/FMA, conversores, ruído analógico, concorrência ou transições. Resíduos de quantização e RMS são resultados sintéticos, não medições no aparelho.

Para novas séries físicas, use os comandos `reverb-map`, `reverb-sweep`, `reverb-controls` e `reverb-transitions` de `Cube.Diagnostics`, seguidos de uma pasta nova. Somente `reverb-map` é exclusivamente de leitura. As demais séries exigem o contexto inicial descrito acima e alteram RAM temporariamente.

Com guitarra, comparar cada ajuste isolado ao original, igualar volume, observar ataques/notas sustentadas, caudas, silêncio após excitação, estalos e mudanças rápidas. Só depois avaliar combinações, exposição de filtros/comprimentos e execução prolongada. Essas avaliações continuam pendentes.
