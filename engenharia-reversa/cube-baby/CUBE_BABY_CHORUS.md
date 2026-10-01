# Cube Baby chorus e controles internos

[Índice](README.md) · [Phaser](CUBE_BABY_PHASER.md) · Investigação de 01/10/2026

## Resultado e alcance

O chorus tem parâmetros internos independentes de MOD: profundidade, incremento de fase, mistura de saída, realimentação, seletor de oscilador e dois filtros biquad. A rotina do aparelho e a tabela de despacho foram lidas e conferidas contra a janela de código preservada na pesquisa do phaser. Duas rodadas físicas, com 20 ensaios ao todo, confirmaram alterações isoladas nesses caminhos e restauração dos campos.

Não houve guitarra, captura de áudio ou avaliação musical. Leituras USB demonstram alteração, permanência e restauração de memória; não demonstram som correto, ausência de estalos, headroom ou estabilidade acústica prolongada. O modelo offline descrito abaixo é uma reconstrução estrutural, não uma gravação nem uma emulação bit a bit.

O criador experimental está integrado ao card Chorus, com sete controles, prévia em RAM, restauração e arquivos locais `.chorus.json`. A investigação e a implementação não definem persistência em flash.

## Fontes e compatibilidade

O ponto de partida foi a [pesquisa do phaser](CUBE_BABY_PHASER.md), que preservou três snapshots por chorus em MOD 0, 3 e 6, e a [pesquisa dos amps](CUBE_BABY_AMPS_CONTROLES_E_ESCUTA.md), que mostrou a importância de distinguir operação matemática, alteração de memória e resultado musical.

O firmware oficial `cubebaby_A_20220706.up` não tem o mesmo layout do aparelho. Por exemplo, `0xC13B04` no arquivo oficial contém outro caminho, enquanto no aparelho é a inicialização dos estados e do buffer de modulação. Não foi usado um deslocamento uniforme para transportar endereços.

| Elemento no aparelho | Endereço CPU | Evidência |
| --- | --- | --- |
| Base do seletor RAM 04 | `0x8347F8` | Perfil e alias já verificados no phaser; reconferidos nesta pesquisa |
| Estrutura de modulação | `0x834E34` | Offset RAM 04 `0x63C`, 88 bytes |
| Aplicação do preset | `0x831AC0` | Tabela indexada por `6 − MOD` para chorus |
| Tabela MOD chorus | `0xC487E4` | Sete pares de floats, profundidade/incremento |
| Filtros iniciais | `0xC487AC` | Dois blocos de sete floats, inicialmente identidade |
| Dispatcher | `0x8327C4` | Usa tabela `0x80C374` |
| Entrada chorus | `0x80C388` | Índice 5, ponteiro Thumb `0x8326B5` |
| Processamento chorus | `0x8326B4` | Corpo e literais, 272 bytes |
| Leitor interpolado do atraso | `0x832188` | Buffer circular de 900 floats |
| Escritor do atraso | `0x832200` | Avança índice e escreve uma amostra |
| Acumulador de fase | `0x83222C` | Soma incremento e corrige uma passagem por 0/1 |
| Tabela de osciladores | `0x80C3DC` | Três ponteiros conferidos; não enumerar valores arbitrários |
| Inicialização de estados | `0xC13B04` | Zera 3600 bytes do buffer e 48 bytes de estados de filtros |
| Biquad | `0x80A91E` | Cinco coeficientes, quatro estados, recorrência confirmada |

Os ensaios exigem as cinco assinaturas do perfil phaser, seis assinaturas adicionais do chorus, entrada 5 do dispatcher e os três ponteiros de oscilador. O nome USB e a identidade textual não bastam. A identidade retornada é comum ao modelo, não um número de série único.

## Mapa dos parâmetros

Todos os offsets abaixo são relativos ao seletor 04. A coincidência de endereços com parâmetros do phaser não implica coincidência de significado.

| Offset | Tipo | Original no chorus | Operação e alcance |
| --- | --- | --- | --- |
| `0x63C` | byte | 3 ativo | Máquina de estados; não é controle musical |
| `0x63D` | byte | 5 | Algoritmo chorus; preservar |
| `0x640` | byte | 0 | Seletor de função do oscilador; 1 é triangular comprovado pelo código |
| `0x644` | float32 LE | 0,4 | Profundidade: multiplica a excursão do atraso por 499 |
| `0x648` | float32 LE | Conforme MOD | Incremento de fase por amostra |
| `0x64C` | float32 LE | 0 | Realimentação: multiplica o ramo processado que retorna ao buffer |
| `0x650–0x663` | 5 floats | 1, 0, 0, 0, 0 | Biquad da média dos atrasos, antes da soma com o sinal direto |
| `0x664–0x66B` | 8 bytes | 0 | Não consumidos como coeficientes pela rotina identificada; preservar |
| `0x66C–0x67F` | 5 floats | 1, 0, 0, 0, 0 | Biquad após combinar direto e atraso |
| `0x680–0x687` | 8 bytes | 0 | Não consumidos como coeficientes pela rotina identificada; preservar |
| `0x688` | float32 LE | 0,85 | Mistura final entre entrada e ramo já combinado |
| `0x68C`, `0x690` | float32 LE | Variáveis | Fases dos dois osciladores; não escritas nos ensaios ativos |

Os coeficientes dos filtros são `b0, b1, b2, a1, a2`, com `y = b0*x + b1*x1 + b2*x2 − a1*y1 − a2*y2`. O helper atualiza estados externos à estrutura. Foi ensaiada somente a redução de `b0` de 1 para 0,5, mantendo os demais coeficientes em zero. Isso confirma dois ganhos independentes; não equivale a validar um editor de frequência/Q ou qualquer combinação de coeficientes.

O oscilador 1 em `0x833540` calcula `2*p` na primeira metade do ciclo e `2−2*p` na segunda. O oscilador 2 retorna a fase diretamente, mas não foi ensaiado fisicamente. O oscilador 0 chama uma função ROM com argumento `2π*p + 3π/2`, soma 1 e divide por 2: a interpretação senoidal é uma inferência estrutural; a função ROM não foi completamente reconstruída.

## Equação do processamento

Para cada amostra, a rotina atualiza as duas fases com o mesmo incremento, consulta dois atrasos e calcula:

```text
d1 = 400 + 499 * profundidade * oscilador(fase1)
d2 = 400 + 499 * profundidade * oscilador(fase2)
atrasado = (ler_buffer(d1) + ler_buffer(d2)) / 2
processado = filtro2((entrada + filtro1(atrasado)) / 2)
escrever_buffer(entrada + feedback * processado)
saída = (1 − mistura) * entrada + mistura * processado
```

As duas fases começam em zero e recebem o mesmo incremento. Nas capturas preservadas elas coincidem; duas leituras de atraso no código não demonstram duas vozes independentes ou estéreo. Não foi encontrada uma configuração estática independente para separação das fases. Modificar diretamente uma fase seria alterar estado dinâmico, fora dos ensaios.

Com os dois filtros identidade e feedback zero:

```text
saída = (1 − mistura/2) * entrada + (mistura/2) * atrasado
original: 0,575 * entrada + 0,425 * atrasado
```

Portanto, **mistura 1 equivale a 50% direto e 50% atrasado**, nessas condições. Mistura 0 fornece a entrada na saída, mas o processamento e o buffer continuam ativos. Não foi demonstrado um controle puro de 100% wet ou vibrato. Aumentar esse campo além de 1 para tentar cancelar o sinal direto não foi ensaiado.

O atraso base 400 e a escala 499 são constantes do código, não campos ajustáveis encontrados na estrutura. Não há controle RAM independente de atraso base demonstrado. Profundidade zero fixa o atraso; não desliga o chorus.

### Limites do buffer

O buffer ocupa `0x80F170–0x80FF7F`, com índice em `0x80FF80`. O leitor usa `posição = índice − atraso − 1`, corrige uma volta negativa e interpola entre duas amostras. O termo adicional de uma amostra deve entrar na tradução para tempo.

Com oscilador no intervalo 0–1 e profundidade 0–0,4, o argumento do leitor fica em 400–599,6 amostras; o atraso efetivo fica em 401–600,6 amostras. A 48 kHz estimados isso corresponde a aproximadamente 8,354–12,513 ms. A faixa ensaiada reduz a excursão original e permanece dentro dos 900 elementos. Valores arbitrários, negativos, NaN ou amplitude de oscilador não demonstrada não são autorizados por essa conta.

Com filtros identidade, interpolação convexa e feedback 0,1, a recorrência admite uma estimativa conservadora de ganho limitado por `1/(1−0,1)`. Essa justificativa matemática orientou o ensaio pequeno; não estabelece headroom ou ausência de clipping no pedal. Qualquer EQ que aumente o ganho exige reavaliar a realimentação.

## Tabela de velocidade

Tabela lida do aparelho e confirmada nos sete valores MOD. Os Hz usam a hipótese estrutural de 48000 amostras/s, já sustentada nas pesquisas anteriores; não foram medidos em áudio.

| MOD | Incremento | Hz estimados |
| --- | --- | --- |
| 0 | 0,000063 | 3,024 |
| 1 | 0,000050 | 2,400 |
| 2 | 0,000040 | 1,920 |
| 3 | 0,000031 | 1,488 |
| 4 | 0,000023 | 1,104 |
| 5 | 0,000015 | 0,720 |
| 6 | 0,000008 | 0,384 |

Profundidade 0,4, mistura 0,85 e feedback zero permaneceram iguais na tabela. A velocidade diminui conforme MOD aumenta dentro do chorus, ao contrário de simplesmente transportar a apresentação do phaser.

## Ensaios USB e recuperação

A captura inicial encontrou PRESET A (`00 01`) e seção delay/modulação desligada. CubeControl e CubeSuite precisam ficar fechados durante o diagnóstico. Uma tentativa de mapeamento foi interrompida antes de abrir a porta porque o aplicativo havia sido aberto; após o usuário confirmar o fechamento, o mapeamento prosseguiu.

A série MOD registrou três snapshots por condição em 0–8, 12 e novamente 3. Entre condições, devolveu MOD e seção ao original e restaurou somente as estruturas inativas. Essa série caracteriza os estados; não é, por si só, uma série de transições diretas entre algoritmos ativos.

Os ensaios de controles usam MOD 3 e duas rodadas dos seguintes valores, sempre um campo por vez:

| Ensaio | Valor experimental | Finalidade |
| --- | --- | --- |
| Profundidade | 0,2 e 0 | Reduzir excursão e fixar atraso |
| Incremento | 0,000008 e 0,000063 | Extremos já observados na tabela |
| Mistura | 0 e 0,425 | Isolar direto e reduzir contribuição do ramo processado |
| Realimentação | 0,1 | Verificar o caminho normalmente zerado |
| Oscilador | 1 | Selecionar o triângulo demonstrado no código |
| Ganho do primeiro filtro | `b0 = 0,5` | Atenuar somente o ramo atrasado antes da soma |
| Ganho do segundo filtro | `b0 = 0,5` | Atenuar o ramo combinado antes da mistura final |

Cada ensaio preserva imagem anterior e intenção de escrita antes do envio. Confere o DSP completo após a escrita, espera 300 ms, reenvia VOLUME com o mesmo valor e MOD 3, e captura novamente. Em seguida muda para MOD 4, verifica quais campos são recarregados, retorna a MOD 3 e restaura o campo original.

O código de aplicação de presets e as duas rodadas concordam: reenviar o mesmo MOD preserva o ajuste; mudar entre valores de chorus recarrega **profundidade e velocidade**, preservando os demais campos ensaiados.

Uma terceira série verificou transições diretas: chorus → phaser → chorus; chorus → MOD 7 → chorus; chorus → MOD 8 → chorus; seção desligada → ligada. Antes de cada transição foi aplicada mistura 0,425. Todas as voltas ao chorus recuperaram os campos estáveis do chorus de fábrica, inclusive mistura 0,85. Esses eventos reinicializam a estrutura e perdem a personalização. Troca física A/B/C, desligamento elétrico e flash não foram ensaiados.

Os comparadores de chorus excluem somente fases `0x68C–0x693` e o float derivado de volume global `0xF6C–0xF6F`. `0x658`, dinâmico no phaser, integra um coeficiente de filtro no chorus e **não** é excluído. Toda outra diferença é tratada como divergência.

Na conclusão de cada série, banco de 48 bytes, estado físico e região DSP de 3952 bytes são comparados à referência. Isso não equivale à restauração bit a bit de toda a RAM: buffer de áudio e estados de filtro externos à região DSP são transitórios e não são reescritos pelo diagnóstico. A inicialização do algoritmo os limpa quando aplicável.

Resultados do restauro: nas séries MOD e controles, os 3952 bytes DSP foram exatamente iguais ao início. Na série de transições, somente `0xF6E`, dentro do float derivado de volume global, diferiu: o float em `0xF6C` passou de 0,046875 para 0,05859375; banco, estado físico e todos os bytes DSP estáveis foram iguais. Esse mesmo campo já variou na captura inicial sem escrita e na pesquisa do phaser. A causa física exata da variação não foi isolada; não houve escrita nesse float para forçar igualdade.

Perda de conexão ou mudança de banco/preset interrompe os envios; nesse caso, os arquivos original e de intenção ficam para recuperação assistida, sem afirmar que o restauro ocorreu. O diagnóstico não oferece recuperação automática após reinício nem um formato de preset de chorus. Não executar novamente em estado desconhecido para tentar forçar uma recuperação.

## Reprodução offline e testes

Foram preservados 159 snapshots nas três séries de escrita, vinculados pelo verificador às respostas USB brutas e aos respectivos banco/estado físico. As 227 escritas têm intenções correspondentes: 79 na série MOD, 129 em controles e 19 em transições. Todos os destinos pertencem a RAM 04 ou parâmetros live 05; nenhuma escrita ou operação de apagamento de flash aparece no tráfego. O segundo mapeamento também é reconstruído a partir das respostas USB e verifica seis assinaturas adicionais de código.

| Artefato | Conteúdo |
| --- | --- |
| [Capturas USB](artefatos/chorus/usb-20261001.zip) | Leitura inicial, mapas, snapshots, intenções e logs integrais |
| [Evidências verificadas](artefatos/chorus/evidence.json) | Hashes, tabela MOD, ensaios, restauro, transições e destinos de escrita |
| [Código desmontado](artefatos/chorus/disassembly.txt) | Rotina, leitor/escritor, osciladores, preset, inicialização e helper biquad |
| [Modelo estrutural](artefatos/chorus/model.json) | Hipóteses explícitas, verificações analíticas e onze cenários sintéticos |

SHA-256 do ZIP: `b24f3ad9271fbc46e550641c7c170699ff1e1e09fad55f1bc50c6b1362effec3`. As capturas são referências históricas; qualquer nova conexão exige verificar novamente o perfil. A desmontagem linear também mostra pools de constantes como instruções; esses trechos não devem ser interpretados como fluxo executável.

O [modelo estrutural](../../scripts/chorus-model.py) usa seno de 440 Hz e impulso, incluindo o atraso adicional de uma amostra. Verifica índices, resultados finitos, limite conservador com feedback e identidades analíticas: mistura zero retorna a entrada; com profundidade zero, filtros identidade e feedback zero, o impulso tem componentes 0,575 em zero e 0,425 em 401 amostras.

O modelo assume 48 kHz, função normalizada senoidal para o seletor 0, fases inicialmente iguais e aritmética Python. Não reproduz arredondamento float32 cumulativo, execução concorrente, conversores, ruído analógico ou limites numéricos do restante da cadeia. Seus RMS e picos são resultados de simulação, nunca medições do pedal.

```powershell
python scripts/chorus-evidence.py engenharia-reversa/cube-baby/artefatos/chorus/usb-20261001.zip --output artifacts/chorus/evidence-recheck.json
python scripts/chorus-model.py --output artifacts/chorus/model-recheck.json
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Tests -- --chorus-only
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Tests -- --phaser-only
```

Nove verificações de chorus cobrem ensaios/restauro, transições, intenções antes de cada escrita, ausência de flash, distinção dos estados dinâmicos, firmware incompatível, ACK sem alteração, mudança física e desconexão. As 18 verificações específicas do phaser também passaram. O verificador offline rejeitou cópias adulteradas de snapshot e de pacote USB. Não foi necessário alterar a interface.

Para reconstruir também a desmontagem, use o mesmo comando de evidência com `--disassembly artifacts/chorus/disassembly-recheck.txt` em um ambiente com `capstone==5.0.9`. A verificação de evidência e o modelo não precisam de dependências externas.

Com o pedal conectado, aplicativos fechados e **uma pasta nova por execução**:

```powershell
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- chorus-map artifacts/chorus/novo-mapa
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- chorus-sweep artifacts/chorus/nova-serie
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- chorus-controls artifacts/chorus/novos-controles
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- chorus-transitions artifacts/chorus/novas-transicoes
```

`chorus-map` somente lê. Os outros comandos alteram RAM, exigem PRESET e seção delay/modulação inicialmente desligada, verificam o firmware e guardam referência e intenções. Não gravam/apagam flash.

## Criador experimental na aplicação

Na Pedaleira, selecione **Chorus** e abra **Criador de chorus · experimental**. Ative o chorus no preset físico em modo PRESET, selecione o mesmo A/B/C no aplicativo e use **Ler chorus ativo**. A leitura confere o firmware, o contexto e os coeficientes dos filtros antes de preservar a referência e habilitar os ajustes.

| Controle | Faixa do editor | Conversão |
| --- | --- | --- |
| Profundidade | 0–100% do original | 0–0,4 |
| Velocidade | 0,384–3,024 Hz estimados | Incremento 0,000008–0,000063 |
| Mistura do ramo processado | 0–100% do original | 0–0,85; não equivale a 0–100% wet |
| Realimentação | 0–10% | 0–0,1 |
| Forma de onda | Original ou Triangular | Seletor 0 ou 1 |
| Ganho do ramo atrasado | 50–100% do original | Primeiro `b0` entre 0,5 e 1 |
| Ganho do ramo combinado | 50–100% do original | Segundo `b0` entre 0,5 e 1 |

As faixas partem da estrutura e dos ensaios isolados; não representam aprovação sonora de todas as combinações. Coeficientes recursivos diferentes de zero impedem iniciar a prévia. Não há edição arbitrária dos demais coeficientes, fases ou ponteiros.

Ajustes rápidos são reunidos e enviados em sequência. A intenção completa é salva antes do primeiro envio; durante uma aplicação de vários campos, o banco, o estado físico e o bloco de modulação são conferidos novamente entre escritas. Antes/depois da aplicação completa, toda a região DSP estável é comparada. A redução de feedback acontece antes dos demais campos; seu aumento vem por último. O monitor ocioso verifica o contexto a cada dois segundos.

Prévia de chorus bloqueia edição do banco e operações concorrentes de phaser, amp e IR. **Restaurar chorus**, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais e confirmam por leitura. Divergência ou perda de conexão interrompem os envios, sem declarar restauração. Recarga por MOD ou troca de algoritmo não reaplica silenciosamente uma personalização.

O registro `pending-chorus-preview.json` e os backups em `ChorusRecovery/`, dentro de `%LOCALAPPDATA%/CubeControl`, preservam referência, perfil, identidade, banco, estado físico e parâmetros anterior/pretendido. Ao reiniciar, a prévia pendente é carregada bloqueada, sem escrita; reconecte o mesmo aparelho e use **Restaurar chorus**. A restauração exige contexto compatível e reconhece escritas parcialmente concluídas. Como a identidade não é um número de série único, esses controles não distinguem absolutamente dois pedais idênticos. Backup inválido bloqueia novas prévias e mantém o arquivo original para diagnóstico.

**Importar chorus** carrega o arquivo local sem enviar; em prévia ativa, **Aplicar ajustes** envia os valores. **Exportar chorus** grava `cubecontrol-chorus-v1`, contendo identidade, versão do mapeamento e sete parâmetros, sem endereços ou imagem bruta. Importação recusa campos desconhecidos, ausentes, valores fora da faixa e mapeamento incompatível. Não há salvamento do chorus na flash nem prévia no dispositivo simulado.

A implementação passou por 28 verificações de protocolo/arquivo/recuperação e 14 de integração desktop. Foram cobertos ACK sem alteração, resposta divergente, firmware incompatível, coeficientes desconhecidos, mudança física entre campos, recarga externa, desconexão parcial, recuperação após reinício, importação local, coalescência, exclusão de outros editores e backup corrompido. As 18 verificações de protocolo e 9 de desktop do phaser também passaram. A interface foi renderizada e inspecionada em português e inglês, incluindo largura compacta. Esses testes usam transporte simulado; os ensaios físicos da investigação anterior continuam registrados separadamente.

```powershell
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Tests -- --chorus-preview-only
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -- --chorus-only
```

## Validação sonora posterior

Realimentação e forma de onda exigem escuta antes de ampliar as faixas musicais. Os dois biquads oferecem um caminho concreto para controles tonais, mas o editor expõe somente os ganhos escalares investigados.

Com guitarra, comparar cada ajuste ao original, igualar volume quando possível, observar ataques e notas sustentadas, mudanças rápidas, profundidade zero, retorno da forma triangular e feedback. Depois avaliar combinações e execução prolongada. Delay base ajustável, vozes independentes, estéreo, 100% wet, outros firmwares e persistência após desligamento permanecem sem demonstração.
