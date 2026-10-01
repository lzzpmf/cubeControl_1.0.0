# Cube Baby — phaser e controles internos

[Índice](README.md) · [Amps](CUBE_BABY_AMPS.md) · Investigação de 01/10/2026

## Resultado

**Foram encontrados e testados controles independentes de MOD:** limites inferior/superior da varredura, nível de saída e número de seções de filtro. A velocidade também pode ser ajustada continuamente dentro dos valores observados. O criador experimental está integrado à tela do phaser, com prévia RAM e arquivos locais `.phaser.json`.

Não houve guitarra, captura de áudio ou avaliação sonora. Os resultados confirmam caminhos de código, alteração isolada, permanência e restauração de memória. Não confirmam qualidade musical, ausência de transientes ou limites acústicos seguros em qualquer combinação.

## Fontes e diferença de firmware

O [ZIP oficial do CubeSuite](https://yms-file-store.oss-cn-hongkong.aliyuncs.com/software/pc/CubeSuite.zip?x-oss-traffic-limit=5192000), recuperado sem executar/instalar seus binários, reproduziu os hashes da pesquisa de amps:

| Fonte | SHA-256 |
|---|---|
| ZIP, 14.785.970 bytes | `d9a1add312ae3fa82f93fefd32a723e541536dd7ed9b2434db364fe8c5a17423` |
| CubeSuite.exe | `0e0183fd6dc1b347534c9ce1e3677ecda2a6ae77fe3da8d6314d43c7c2cfd2e6` |
| BFI0615.bin | `4f78d5d8d967276a6f069b52887df87f9e8ce56a53dc89b368a2e335aeb53f97` |
| cubebaby_A_20220706.up | `d99a9bc4ba0636118801b1e3a8d11adadf03831d316c7477af8de015ba5bd743` |

Entretanto, **o código lido do aparelho não corresponde ao mesmo layout de execução da atualização oficial**. O primeiro diagnóstico tentou conferir a assinatura da pesquisa de footswitches e parou, sem escrita. O retorno coincidiu com outro trecho do firmware; uma hipótese inicial de deslocamento uniforme também foi rejeitada por outras assinaturas.

Uma captura limitada de `04:0xFFFFAC00`, comprimento `0x5400`, preservou a janela em torno dos trechos de código já retornados. A comparação mostrou rotinas semelhantes em posições diferentes e mudanças reais no controle de ativação. Não é um dump completo do firmware e não identifica uma versão comercial.

| Elemento | Arquivo oficial | Código do aparelho nesta sessão |
|---|---|---|
| Base CPU do seletor 04 | `0x834660` | `0x8347F8` |
| Leitor do banco | `0x83244C` | `0x832108` |
| Banco A/B/C em RAM CPU | `0x80CBB8` | `0x80CA74` |
| Seleção/modo CPU | `0x80CB12` | `0x80C9FA` |
| Rotina de processamento do phaser | `0x83343C` | `0x83239C` |
| Despacho da modulação | `0x833864` | `0x8327C4` |
| Estrutura de modulação CPU | `0x834C9C` | `0x834E34` |
| Estrutura relativa ao seletor 04 | `0x63C`, 88 bytes | `0x63C`, 88 bytes |

A base atual foi deduzida por chamadas para código ROM e corroborada por literais do chamador DSP, leitor do banco, alias igual ao banco normal e tabela de despacho apontando para o worker correto. **Não houve simplesmente aplicação de um delta a todos os símbolos.**

O perfil implementado, `cube-baby-live-20261001-8347f8-v1`, confere hashes completos de cinco trechos: leitor do banco (12 bytes), worker (260), inicialização (70), seleção/aplicação de presets (440) e dispatcher (136), além da entrada do dispatcher. O nome USB ou a identidade textual não habilitam escrita por si só. O arquivo oficial antigo não foi habilitado como segundo perfil sem testes físicos.

## Caminho de MOD até o DSP

No arquivo oficial, `0x831E9C` aplica o preset e `0x832180` inicializa o phaser. No aparelho, os caminhos correspondentes passam por `0x831AC0` e `0x831D84`. A seção do preset no offset `0x0B` habilita tanto delay quanto modulação. MOD 7/8 desliga a modulação; 0–6 configura chorus; 9–15 seleciona phaser.

O chamador atual `0x833724` passa `0x834E34` ao dispatcher `0x8327C4`. Este usa a tabela CPU `0x80C374`; a entrada 2, lida do aparelho, contém `0x83239D` (ponteiro Thumb para o worker `0x83239C`). O estado ativo observado nesta versão é **3**, diferentemente da versão oficial analisada. Os estados de transição não são controles editáveis.

| Seletor 04 | Tipo | Valor original observado | Uso pelo worker / decisão |
|---|---|---|---|
| `0x63C` | byte | 3 ativo / 0 inativo | Estado de ativação; preservar |
| `0x63D` | byte | 2 | Algoritmo phaser; preservar |
| `0x640` | byte | 1 | Seletor da função do oscilador; não exposto nesta versão |
| `0x641` | byte | 2 | Número de seções = `min(byte + 1, 3)`; editor expõe 1–3 |
| `0x644` | float32 LE | Conforme MOD | Incremento de fase por amostra; velocidade |
| `0x648` | float32 LE | 5000 | Participa do cálculo de coeficientes; preservado, sem controle de largura exposto |
| `0x64C` | float32 LE | 300 | Primeiro extremo da interpolação da varredura |
| `0x650` | float32 LE | 3000 | Segundo extremo da interpolação da varredura |
| `0x654` | float32 LE | 0,64 | Multiplica a soma do sinal direto com o filtrado; nível, **não mistura dry/wet** |
| `0x658` | float32 LE | Variável | Fase do oscilador; estado dinâmico, nunca editado pelo criador |

A rotina soma o incremento à fase, calcula uma função periódica e interpola os extremos: `inferior + oscilador × (superior − inferior)`. O cálculo de coeficientes usa constantes π, 2π e aproximadamente `1/48000`. A estrutura é compatível com seções biquad passa-tudo. As funções trigonométricas chamadas na ROM são inferidas pela forma das equações; a imagem não contém essa ROM completa.

O número de seções é limitado a três pelo próprio código, que usa estados separados com stride de 16 bytes. A saída tem a forma `1,5 × nível × (entrada + filtrado)`. Não foi identificado neste caminho um controle independente de feedback musical ou de mistura dry/wet. A alteração dos extremos controla a extensão da varredura, sem inventar um campo adicional chamado “depth”.

### Velocidade: a tabela do aparelho difere da atualização

| MOD | Incremento observado no aparelho | Hz estimados a 48 kHz |
|---|---:|---:|
| 9 | 0,00001 | 0,48 |
| 10 | 0,00002 | 0,96 |
| 11 | 0,00003 | 1,44 |
| 12 | 0,00005 | 2,40 |
| 13 | 0,00008 | 3,84 |
| 14 | 0,00010 | 4,80 |
| 15 | 0,00013 | 6,24 |

Na atualização oficial, a tabela em `0xC48CC4` contém incrementos de 0,00001 a 0,00005, com passos intermediários diferentes. A interface usa os limites **medidos no aparelho**, não a tabela antiga. As unidades em Hz são estimativas estruturais, sem medição de áudio.

## Ensaios físicos e restauração

O usuário confirmou modo PRESET e LED A. Leituras do novo alias retornaram `00 01`, coerentes com essa confirmação; não foi conduzida aqui uma nova série manual A/B/C/bypass. A porta apareceu como `USB-Midi [1]` na entrada e `USB-Midi [0]` na saída. O diagnóstico aceita somente uma porta de cada direção com esse nome, incluindo o sufixo numérico do Windows.

O banco inicial tinha MOD A=7 e seção delay/modulação A=0. Foram preservados banco, DSP e estado físico antes de ligar temporariamente a seção. A primeira captura repetida sem alterações retornou três imagens DSP idênticas.

1. **Série MOD:** 7, 8, 9–15, 0, 3 e 6, três snapshots por valor. Restauro do MOD, seção e estruturas inativas entre valores. Os sete phasers mantiveram extremos 300/3000, nível 0,64 e três seções. Só a velocidade acompanhou MOD.
2. **Controles extras:** MOD fixo em 12. Duas rodadas de sete ensaios: incremento 0,00001; incremento 0,00013; extremo inferior 1000; extremo superior 2000; nível 0,32; uma seção; duas seções. Cada ensaio alterou somente um campo.
3. **Permanência:** leitura imediata, espera de 300 ms, reenvio do VOLUME A com o mesmo valor e nova leitura. Os parâmetros continuaram nos valores previstos.
4. **Restauro por ensaio:** campos originais conferidos, preservando o banco e os demais bytes estáveis. Nenhuma escrita na fase do oscilador.
5. **Encerramento:** MOD=7, seção=0 e estruturas inativas devolvidos ao início. Na última série, banco de 48 bytes, estado físico e todos os **3952 bytes** DSP lidos foram idênticos ao início dessa série.

Na primeira série, o comparador integral acusou diferença somente em `0xF6E`, dentro do float de volume global em `0xF6C`. O código de aplicação do preset recalcula esse campo a partir do volume e do controle global; houve também variação durante leituras repetidas. A causa exata da variação física não foi isolada. O comparador atual separa esse float derivado e a fase `0x658–0x65B` das regiões estáveis. **Não se escreveu nesse volume para forçar igualdade.** O script de evidência verifica a diferença explicitamente e rejeita qualquer diferença estável não prevista.

Foram registrados 86 comandos de escrita na série MOD e 51 na série de controles. Todos foram `0x22` em RAM 04 ou parâmetros live 05; o arquivo de evidências valida os destinos e confirma ausência de escrita/apagamento em flash. ACK não foi usado como confirmação suficiente.

## Criador experimental e recuperação

Na Pedaleira, selecione Phaser e abra **Criador de phaser · experimental**. Ative o phaser e a seção delay/modulação no preset físico, selecione o mesmo A/B/C na aplicação e use **Ler phaser ativo**. Esta ação verifica o perfil e guarda a referência antes de habilitar a prévia.

- Velocidade: limites observados 0,48–6,24 Hz estimados, ajuste contínuo.
- Extremos: 300–3000 Hz estimados, inferior ≤ superior. Esses limites restringem a varredura ao intervalo original; não são uma medição de resposta acústica.
- Nível: 0–100% do multiplicador original 0,64; não oferece amplificação além do original.
- Seções: 1–3, conforme o limite e o espaço de estados usados pelo worker.

Mudanças rápidas são reunidas e enviadas em sequência. Apenas os cinco campos aprovados podem ser escritos. O par de extremos é atualizado na ordem que mantém o intervalo válido durante a transição. Antes/depois de cada envio são conferidos banco, contexto e toda a região DSP estável. Um monitor periódico verifica contexto e bloco de modulação enquanto a prévia está ociosa.

O criador bloqueia prévias concorrentes de amp/IR e edições de preset. **Restaurar phaser**, desconexão normal, releitura do banco e fechamento normal tentam restaurar os parâmetros. Perda de conexão ou divergência interrompem os envios e preservam `pending-phaser-preview.json` em `%LOCALAPPDATA%/CubeControl`; o backup inicial também fica em `PhaserRecovery/`. O registro inclui o valor anterior e o pretendido antes de cada escrita, permitindo reconhecer transações parcialmente aplicadas.

Ao reiniciar, uma prévia pendente é carregada para recuperação explícita, sem reaplicar ajustes. A restauração exige o mesmo perfil, identidade, banco e contexto físico. Essa identidade é a resposta comum do modelo, **não um número de série único**; as verificações adicionais reduzem enganos, mas não provam identidade física entre dois pedais idênticos. Se o contexto não corresponder, o programa não sobrescreve um estado desconhecido.

Arquivos `.phaser.json` usam `cubecontrol-phaser-v1`: identidade, versão do mapeamento e cinco parâmetros, sem endereços arbitrários ou imagem de amp. Importar carrega a edição local; em uma prévia ativa, **Aplicar ajustes** envia o arquivo. Exportar não escreve no pedal. Não há salvamento persistente de phaser nesta versão, nem suporte de prévia no dispositivo simulado.

## Evidência reproduzível e testes

- [Capturas USB completas](artefatos/phaser/usb-20261001.zip), SHA-256 `10324773859b09fe949008d22eb6fbc7fd4a673797429500f341fa6f77cda985`.
- [Janela de código do aparelho](artefatos/phaser/live-code-20261001.bin), SHA-256 `91da48cbe86aedca0debc4f08fa538e9193bfde46960af5c550b2ae1963708f6`.
- [Manifesto, tabelas e verificações](artefatos/phaser/evidence.json), reconstruível sem dependências externas:

```powershell
python scripts/phaser-evidence.py engenharia-reversa/cube-baby/artefatos/phaser/usb-20261001.zip --output artifacts/phaser/evidence-recheck.json
```

Para desmontar os trechos conhecidos, use Python com `capstone==5.0.9` em ambiente local. O script exige o hash correspondente e não se comunica com o pedal:

```powershell
python scripts/phaser-firmware.py --live engenharia-reversa/cube-baby/artefatos/phaser/live-code-20261001.bin disasm 0x83239C 0x104
python scripts/phaser-firmware.py CAMINHO/cubebaby_A_20220706.up disasm 0x83343C 0x104
```

Os comandos de diagnóstico abaixo exigem aplicação/CubeSuite fechados e uma pasta nova. `phaser-snapshot` e `phaser-map` somente leem. `phaser-sweep` e `phaser-controls` **alteram RAM**, exigem slot físico confirmado, modo PRESET e seção delay/modulação inicialmente desligada; preservam snapshots antes dos ensaios. Em perda de conexão, não prometem restauração e mantêm os arquivos da série para recuperação assistida.

```powershell
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- phaser-snapshot artifacts/phaser/nova-captura
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- phaser-sweep artifacts/phaser/nova-serie 0
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -- phaser-controls artifacts/phaser/novos-controles 0
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Tests -- --phaser-only
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -- --phaser-only
```

Validação automatizada: 18 verificações de protocolo/arquivo/recuperação e 9 de integração desktop. Cobrem ACK sem mudança, timeout, resposta com endereço divergente, firmware incompatível, recarga externa, mudança física, desconexão parcial, restauração após reconexão/reinício, coalescência e exclusão de outras operações. A interface foi renderizada e inspecionada. A suíte desktop existente também concluiu 44 verificações.

A suíte geral de protocolo para em `ExtendedTests.cs`, teste `one transaction and minimum 100 ms interval`. Isso já diverge da implementação em HEAD: `LiveEditQueue` serializa, mas não impõe espera de 100 ms. Não foi modificado o comportamento dessa fila para fazer um teste antigo passar. Os testes específicos do phaser passam independentemente dessa falha.

## Roteiro de escuta posterior

Com guitarra disponível, comparar original e uma alteração por vez, incluindo ataques fracos/fortes e notas sustentadas. Conferir velocidade percebida, largura/posição da varredura, volume e efeito das seções; testar os extremos e combinações somente após os controles isolados. Observar estalos ao mudar seções, transientes, saturação e estabilidade em execução prolongada. Repetir restauração e mudanças de preset/algoritmo. Somente depois revisar faixas musicais, rótulos e a condição experimental.

Ainda não foram validados waveform alternativo, campo 5000, feedback, mix independente, armazenamento de phaser na flash, outros firmwares ou persistência dos ajustes após desligamento. Nenhum desses recursos é anunciado pelo criador.
