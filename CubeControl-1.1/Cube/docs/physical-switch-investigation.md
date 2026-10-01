# Investigação dos switches físicos — 19/09/2026

[Documentação](README.md) · **Pesquisa histórica, sem suporte ativo no produto** · Revisão: 01/10/2026

Na árvore atual, `CubeSession.Footswitch.cs` e `EditorViewModel.Footswitch.cs` permanecem no código, mas `StartFootswitchMonitoring` não é chamado. A/B/C continuam representando seleção de edição. Os executáveis 3.3.68/3.3.69, logs em `windows/artifacts`, checkout `.tools/cuvave-midi`, firmware e script `windows/research/firmware_disasm.py` citados abaixo não estão incluídos neste repositório.

## Estado atual: investigação adiada

O usuário testou a versão experimental 3.3.69 e informou que não funcionou.
A pedido dele, naquela etapa o inicializador voltou ao executável preservado 3.3.68 e o
monitoramento experimental foi retirado da inicialização do aplicativo.
O código e as evidências de pesquisa foram mantidos para investigação futura.
Não tratar a hipótese de leitura como validada no hardware.

Hardware identificado ao vivo: `SINCO-CubeBaby`, porta `USB-Midi`. Nenhuma escrita de preset, IR ou configuração foi enviada durante esta investigação.

## Ensaios realizados com o usuário

1. Captura de mensagens USB brutas e leitura do banco de 48 bytes enquanto o usuário pressionou A, B e C e confirmou as respectivas luzes. Não foi recebida mensagem espontânea de seleção. O banco lido permaneceu idêntico. Evidência: `windows/artifacts/physical-switches.log`.
2. Leitura da região 4, endereço 0, 128 bytes, enquanto o usuário executou A → B → C → A. Houve mudanças no estado de áudio, mas nenhum índice de preset foi validado. Evidência: `windows/artifacts/physical-state.log`.
3. Capturas separadas, com o usuário mantendo a luz A, B e C acesa, respectivamente. Região 4, intervalo `[0, 0x764)`, em blocos de até 128 bytes. Evidências: `windows/artifacts/switch-state-A.log`, `switch-state-B.log` e `switch-state-C.log`.

## Resultados

- A consulta `0x12` devolveu `01 00 00 00 00 00` nas três posições; não diferencia A/B/C.
- O primeiro byte da região 4 permaneceu `02` nas três posições. Não deve ser interpretado como preset C.
- O byte `0x11` da região 4 foi `7 / 2 / 7`, coincidindo com os ganhos dos presets capturados. Não identifica o switch: A e C compartilham o valor.
- Muitos coeficientes de processamento mudam entre presets. Isso demonstra mudança do estado de áudio, mas não comprova um campo de seleção independente dos efeitos.
- No intervalo examinado não apareceu uma sequência de bytes que validasse um índice A/B/C `0/1/2` ou `1/2/3`.

A referência `cuvave-midi` usa os bits superiores do **endereço de parâmetro** para distinguir os bancos A/B/C. Isso não é um registro do switch ativo. O parser `PresetParameterPair` confirma essa distinção.

## Situação da implementação

A sincronização teve uma implementação experimental, descrita ao final, mas não foi validada pelo usuário nem permanece ativa no produto. As capturas não provam impossibilidade no firmware inteiro; delimitam apenas as mensagens e a área consultadas. É necessário validar uma consulta/registro confiável de seleção, incluindo bypass e modos LIVE/EDIT. Comparar ganho ou parâmetros isolados pode selecionar a cadeia errada quando dois presets compartilham valores.

O diagnóstico ganhou os comandos `observe-switches`, `observe-state` e `snapshot-state`. Execute-os com o editor fechado, evitando respostas concorrentes. Todos usam somente identificação, consultas ou leitura. Os logs acima ficam nos artefatos locais; não contêm uma definição reutilizável de protocolo para seleção física.

Referência examinada: [cuvave-midi](https://github.com/pferreir/cuvave-midi), disponível em `.tools/cuvave-midi` na máquina histórica, não neste checkout. A análise estática do CubeSuite original não forneceu um mapeamento adicional confirmado nesta investigação.

## Avanço: análise direta do firmware de atualização

Imagem local: `CubeSuite/bin/up/cubebaby_A_20220706.up`, 504600 bytes, SHA-256
`d99a9bc4ba0636118801b1e3a8d11adadf03831d316c7477af8de015ba5bd743`.
Nenhuma atualização foi instalada. A análise usa Capstone Thumb em
`windows/research/firmware_disasm.py`.

Mapeamentos confirmados por referências e rotina de cópia:

- XIP: endereço virtual = offset no arquivo + `0xBFFF00`.
- RAM: trecho flash `0xA900`, tamanho `0x5400`, copiado para `0x830C00`
  pela rotina `0xC49C04`. No arquivo, o trecho começa em `0xAA00`;
  endereço virtual = offset no arquivo + `0x826200` (`--ram` no script).

Evidência estática forte de seleção, independente dos parâmetros:

| Endereço RAM | Significado deduzido do código |
| --- | --- |
| `0x80CB12` | Índice selecionado: 0, 1, 2; `0xFF` ao desligar o preset já ativo |
| `0x80CB13` | Modo: 1 usa presets; 0 usa controles LIVE; outros valores exigem tratamento separado |
| `0x80CBB8` | Banco de três presets, 16 bytes por preset |
| `0x80CB70` | Máscara de LEDs, também usada para indicadores e animações |

As rotinas em `0xC138D0`, `0xC13964`, `0xC139FC` alteram `0x80CB12`
para 0/1/2, ou `0xFF` quando o mesmo índice já está selecionado. Elas são
chamadas pela tabela dos botões físicos. A rotina `0x832264`, no modo 1,
lê esse índice e aplica `bank + index * 16`; acima de 2 aplica configuração
alternativa. A rotina de LEDs `0x8319CC` usa o mesmo índice. Isso explica por
que as capturas anteriores na região DSP não encontraram o seletor.

O dispatcher USB em `0xC49CB8` aceita identificação `0x11`, resposta estática
`0x12`, apagar `0x21`, escrever `0x22` e ler `0x23`. Não foi encontrada uma
assinatura/ativação de notificações nessa rotina. A busca de referências
diretas no código encontrou somente o dispatcher chamando o transmissor
`0x8316BC`; nenhuma referência direta partindo das rotinas de footswitch.
Isso é evidência estática de ausência de notificação nesse caminho, não uma
prova universal sobre todos os firmwares ou chamadas indiretas.

### Consulta candidata, apenas para pesquisa

O leitor da região 4 soma o offset de 32 bits à base `0x834660`, sem checagem
de limites nesse firmware. Portanto um offset com soma modular permite
consultar os dois bytes em `0x80CB12`, fora do bloco DSP:

`CMD=0x23, região=4, offset=0xFFFD84B2, tamanho=2`.

O diagnóstico `firmware-state LOG SEGUNDOS` foi preparado com verificações:

1. Editor e CubeSuite devem estar fechados.
2. Lê 12 bytes de código em `0x83244C` (região 4, offset `0xFFFFDDEC`)
   e exige assinatura `014B194436F4B6B3B8CB8000`.
3. Compara banco normal (região 5, offset 0) e alias RAM
   (região 4, offset `0xFFFD8558`), ambos com 48 bytes.
4. Somente depois lê seleção/modo a cada 250 ms; no final compara o banco.

Não envia `0x21`, `0x22` ou `0x24`. Os endereços não constituem API pública
do fabricante: não devem ser habilitados para qualquer firmware com base
apenas no nome USB. A correspondência do hardware e a sequência física
A/B/C ainda precisam ser validadas no hardware.

## Histórico da publicação experimental 3.3.69, posteriormente retirada

Por solicitação do usuário, a consulta foi integrada ao aplicativo e publicada
em `windows/artifacts/CubeControl-3.3.69-footswitch`. O launcher apontou
temporariamente para esse diretório; o teste do usuário falhou e a mudança foi retirada, conforme o início deste documento.

`CubeSession.ReadFootswitch` verifica a assinatura e o alias do banco uma vez
por conexão. Firmware divergente desativa a consulta, com aviso ao usuário.
Na implementação experimental, o editor consultava a cada 500 ms enquanto disponível, sem gesto ativo,
sem prévia de IR e sem alterações aguardando envio. Uma transição válida
de seleção em modo PRESET altera `SelectedIndex`, reaproveitando as cadeias
já carregadas. Não escreve nem salva no pedal. Navegação manual não é
sobrescrita enquanto o estado físico continua igual; bypass/LIVE não são
interpretados como um dos três presets. Edições locais são preservadas.

Testes simulados cobrem A/B/C com presets idênticos, navegação manual,
bypass/LIVE, gestos, alterações pendentes/não salvas, firmware divergente,
alias divergente e timeout. Esses testes não substituem a validação física.
