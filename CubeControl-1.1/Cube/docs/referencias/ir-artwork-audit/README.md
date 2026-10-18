# Auditoria das imagens de IRs — 18/10/2026

Escopo: todos os 958 WAVs distribuídos em `Assets/ImpulseResponses` e as 16 identidades BFI/BFIN. Não inclui arquivos externos importados nem substitui imagens personalizadas. O [inventário por arquivo](inventory.json) registra SHA-256, evidência, confiança, imagem anterior/final e motivo. Esta é uma auditoria de metadados e apresentação, não identificação acústica do equipamento.

## Resultado por família

| Família | WAVs | Configuração | Decisão |
| --- | ---: | --- | --- |
| Zilla Fatboy / GOV | 127 | 2×12 | Nova arte horizontal; antes recebia fallback de um falante |
| DE-112 / E112 | 329 | 1×12 com duto | Nova arte sem logotipo; fabricante Engl continua provável |
| M-212 | 187 | 2×12 | Nova arte britânica vintage sem logotipo; modelo comercial não confirmado |
| R-112 / RECTO-112 | 39 | 1×12 | Nova arte separada do Rectifier 4×12 |
| Fender Deluxe Oxford | 4 | 1×12 | Mantida |
| Fender Twin Reverb | 3 | 2×12 | Mantida |
| Marshall 1960 Custom | 1 | 4×12 | Mantida |
| Marshall 1960AV | 4 | 4×12 | Mantida |
| Marshall 1960TV Greenback | 72 | 4×12 | Mantida |
| Mesa 4FB | 168 | 4×12 | Mantida |
| Mesa Oversized | 1 | 4×12 | Mantida |
| Mesa Rectifier sem configuração | 1 | Não informada | Removida suposição de 4×12; frente opaca neutra |
| Orange V30 | 20 | 2×12 horizontal | Mantida |
| Vox AC30 Blue | 2 | Referência 2×12 | Mantida como ilustração do modelo, sem elevar a certeza da captura |

Os oito BFI mantêm as ilustrações e configurações de referência descritas em [características dos cabinets](../cabinet-characteristics.md). Os oito BFIN passam de um fallback visivelmente 1×12 para `cabinet-unknown.png`: o hash identifica o conteúdo, mas não a caixa da captura.

Foram corrigidas 314 associações de WAVs 2×12 que usavam a imagem genérica de um falante. Outros 368 WAVs recebem arte específica de sua família 1×12. Um WAV desconhecido e oito BFIN deixam de afirmar uma configuração não documentada. Nenhum WAV, hash de identificação, slot ou parâmetro de áudio foi alterado.

## Evidências e limites

- [Zilla Fatboy](https://www.zillacabs.com/fatboy): referência oversized 2×12; acabamento da arte ilustrativo.
- [Overdriven UK-212/GOV](https://overdriven.fr/overdriven/index.php/uk212-gov-home-studio-edition-free-guitar-cab-impulse-responses-download/): coleção e readme local sustentam a família FB/GOV.
- [Overdriven DE-112/T75](https://overdriven.fr/overdriven/index.php/de-112-t-75-g4-free-guitar-cab-impulse-responses-download/): caixa alemã 1×12 com montagem frontal e duto; marca não declarada nessa página.
- [Overdriven M-212/GB55](https://overdriven.fr/overdriven/index.php/m-212-gb55-g4-free-guitar-cab-impulse-responses-download/): gabinete britânico vintage 2×12; não confirma modelo comercial Marshall.
- [Overdriven R-112/V30](https://overdriven.fr/overdriven/index.php/r-112-v30-g4-free-guitar-cab-impulse-responses-download/): família R-112; evidência adicional nos readmes e metadados locais.

Os 682 registros Overdriven preservam seus estados: 166 confirmados e 516 prováveis. Fontes por coleção constam no inventário. Nas demais famílias, nomes e diretórios sustentam ilustrações de referência, não provas do equipamento efetivamente capturado. As representações Fender/Vox são estilizações do gabinete, não fotografias certificadas de combos. Não acrescentamos cabeçotes ou microfones sem evidência.

## Resolução no app

`CabinetArtwork` centraliza identidades BFI/BFIN e escolha por metadados/caminho/nome. Configuração estruturada tem precedência sobre texto; regras de modelo não podem contradizê-la. “Mesa Rectifier” sozinho não basta para escolher quatro falantes. Configurações conhecidas sem modelo usam ilustrações sem logotipos compatíveis com a contagem; outras configurações e identidades desconhecidas usam frente opaca. Códigos de família continuam disponíveis quando o catálogo estiver ausente.

Pedaleira e galeria usam a mesma resolução de identidade; packs consultam também os metadados da biblioteca. A precedência das imagens personalizadas permanece em `WithImage`. O desenho de distância do microfone usa a frente neutra: é um diagrama de gesto, não uma identificação do IR.

## Assets e revisão

As cinco artes novas ficam em `windows/Cube.Desktop/Assets/Images/Equipment/Cabinets`: `zilla-fatboy-2x12.png`, `de112-ported-1x12.png`, `m212-vintage-2x12.png`, `mesa-rectifier-1x12.png` e `cabinet-unknown.png`. Produzidas com ImageGen integrado; [prompts](generation-prompts.json). Assets anteriores preservados para referências explícitas e personalizações.

Foi inspecionada a [prancha das 21 imagens anteriores](existing-cabinets.jpg): contagem/layout, enquadramento e família. As oito imagens BFI são gabinetes sem os cabeçotes sugeridos por alguns nomes históricos. Versões verticais 2×12 e imagem genérica antiga permanecem nas associações apropriadas; DirectBox representa OFF. `Selection` contém opções manuais, não associações automáticas aos IRs.

Novas artes inspecionadas individualmente e em [prancha sobre fundo claro](new-cabinets.jpg): dois/um/dois/um falantes, respectivamente, e nenhum cone aparente na neutra. Canal alfa e dimensões inspecionados; o RGB oculto dos PNGs não deve ser confundido com pixels opacos. Silhuetas inteiras. Arte gerada não comprova marca, acabamento ou falante real.

Revisão estática de referências e cobertura realizada. Não foram executados testes, compilação ou gravação no pedal. Validação visual dentro do app, em temas e escalas diferentes, permanece pendente.
