# Controles físicos e reconhecimento pelo aplicativo — 08/10/2026

## Resultado

No pedal conectado, identificado como `SINCO-CubeBaby` e perfil de código
`cubebaby_B_20220706`, é possível observar mudanças físicas por consultas de
memória USB. Não foram recebidas mensagens espontâneas na janela passiva
testada. Isso sustenta investigação de sincronização por polling, mas não uma
implementação completa nem ausência universal de notificações.

## Captura passiva

Foi feita identificação para conferir o caminho de recebimento e, depois,
60 segundos de escuta de todos os bytes MIDI recebidos, sem consultas ou
escritas durante a janela. O participante confirmou ter girado knobs e
pressionado A/B/C em PRESET e LIVE.

Resultado: **zero mensagens brutas recebidas** durante a janela. O teste não
examina outros firmwares, um eventual comando de habilitação de notificações,
nem outros transportes.

Evidências locais: `tmp/physical-inputs-20261008-passive-1/transport.log` e
`summary.json`. Os timestamps são UTC; a sessão ocorreu em 08/10 no fuso de
São Paulo (o UTC já indicava 09/10).

## Captura por consultas

Comandos enviados: identificação `0x11` e leituras `0x23`. Foram verificadas as
assinaturas de firmware e o alias do banco pelo método `ReadPhaserResearch`.
Foram preservadas 37 amostras estáveis em aproximadamente 121 segundos; algumas
leituras atravessaram transições e foram descartadas pelas verificações já
existentes. A leitura completa leva aproximadamente 1,9 segundo, além da pausa
de 1 segundo; **este ensaio não demonstra latência de 250 ms**.

| Instante aproximado | Observação |
| --- | --- |
| 34,9 s | DSP `04:0x10` passou de `00` para `03`, e `04:0x11` de `00` para `05`, em LIVE |
| 37,8 s | DSP `04:0x11` passou de `05` para `07`, ainda em LIVE |
| 59,6 s | Seleção/modo `00 01`, compatível com PRESET A |
| 62,6 s | Seleção/modo `01 01`, compatível com PRESET B |
| 71,6 s | Seleção/modo `02 00`, LIVE com byte de seleção 2; não afirmar PRESET C ativo |
| 101–112,7 s | Mudanças em regiões DSP do delay (`0x694…`) e reverb (`0x6E0…`), em LIVE |

O banco A/B/C de 48 bytes permaneceu igual em todas as amostras estáveis.
Portanto, **consultar apenas esse banco não acompanha os knobs em LIVE**.
Os controles foram movimentados pelo participante, mas não houve marcação
individual de cada gesto; não associar todos os bytes alterados a um knob
específico. Coeficientes DSP e estado dinâmico não são necessariamente posição
física do potenciômetro.

Evidências locais: `tmp/physical-inputs-20261008-state-1/initial.json`,
`sample-001.json` até `sample-037.json`, `transport.log` e `summary.json`.
Os arrays de bytes nesses JSON são codificados em Base64.

## Consequências para o cubeControl

- O acompanhamento teria de consultar registros de estado/parâmetros e atualizar
  a interface quando eles mudarem. O firmware responde à consulta, sem evento
  espontâneo observado neste ensaio.
- A antiga consulta de footswitch usa outro layout (`0xFFFD84B2`) e seu timer
  não é iniciado. Os perfis mais recentes usados nos editores de efeitos têm
  seleção/modo em offsets próprios (`0xFFFD8202` e `0xFFFD81F6`). Não basta
  reativar o timer antigo.
- Para este perfil B, a consulta de seleção/modo é região 4, offset
  `0xFFFD81F6`, tamanho 2, após verificar compatibilidade.
- Ainda faltam sequência estável completa A/B/C/bypass, identificação individual
  dos knobs em LIVE/EDIT/PRESET, switches de seção e interação com alterações
  locais e prévias do editor. A captura não validou todos esses casos.
- Consultas curtas são candidatas a reduzir tráfego e latência; precisam de
  ensaio próprio. A sincronização deve usar a fila da sessão, preservar edições
  locais e impedir que uma atualização da interface dispare escrita de retorno.

Nenhuma escrita de preset, IR, DSP ou firmware foi enviada. O produto permanece
sem sincronização automática; foram acrescentados somente comandos de pesquisa.

## Repetição

Com CubeControl e CubeSuite fechados, executar o projeto `Cube.Diagnostics`:

```text
observe-physical-inputs PASTA-NOVA 60
observe-physical-state PASTA-NOVA 120
```

O primeiro identifica e escuta sem consultas durante a janela. O segundo verifica
o perfil e captura banco, DSP e seleção/modo somente por leituras. Usar pastas
novas para preservar evidências. A compilação e ambos os comandos foram
executados no hardware desta sessão.

Referência externa consultada: [parser cuvave-midi](https://github.com/pferreir/cuvave-midi/blob/main/src/lib.rs),
que descreve leitura/conteúdo de memória e parâmetros; o resultado físico acima
vem das capturas desta sessão, não de uma inferência a partir desse parser.
