# Biblioteca Overdriven.fr

A biblioteca contém 13 coleções e 682 WAVs, preservados byte a byte em
`windows/Cube.Desktop/Assets/ImpulseResponses/Overdriven.fr`. O manifesto
`Overdriven.catalog.v1.json` é independente dos packs de oito slots.

## Atualização reproduzível

Na raiz do workspace, execute `python scripts/Generate-OverdrivenCatalog.py`.
Use `--check` para conferir catálogo, WAVs e readmes sem escrever arquivos.
A ferramenta lê os WAVs e readmes versionados em `Assets/ImpulseResponses/Overdriven.fr`
e os mapeamentos revisados em `scripts/overdriven_metadata.py`. Não depende de
`irsNovos`, não baixa arquivos e não escreve nem processa áudio. `--check`
recalcula hashes, formatos e classificações e compara o resultado ao manifesto.

O manifesto v1 registra caminho relativo à biblioteca, SHA-256, coleção,
caixa/falante descritos no readme, potência/pré de microfone, código/modelo de
microfone, posição, distância aproximada em mm, variantes, prefixo de captura,
preview, nome original e formato técnico real. A identidade da coleção é o
nome original da pasta; CV e VT permanecem distintas. Não se deve renomear os
caminhos após criar packs que os referenciem.

## Catálogo unificado

A Biblioteca e o seletor de packs compartilham a árvore **marca do gabinete →
configuração de falantes → modelo e falante → captação**, sem picklists. A busca
combina termos de marca, configuração, modelo, microfone, posição, nome original
e origem. Overdriven.fr é a origem, não uma marca de gabinete na árvore.

O manifesto mantém a versão 1 com campos adicionais `Brand`, `SpeakerConfiguration`,
`CabinetModel`, `SpeakerLabel`, `Identification`, `IdentificationEvidence` e `Sources`.
Identificações podem ser `confirmed`, `probable` ou `unknown`. Catálogos antigos
continuam legíveis, sob “Marca não identificada”; catálogos inválidos usam as pastas.
Os rótulos são somente apresentação: os caminhos, hashes, nomes originais e dados
de captura permanecem independentes das classificações. Variantes desconhecidas
continuam literais; nomes de captação coincidentes recebem o nome original ou caminho
para distinguir arquivos. Não há deduplicação por nome ou por conteúdo.

Revisão das 13 coleções em 10/10/2026 (fontes específicas e readme em cada entrada):

| Coleção | Marca | Falantes | Modelo / falante | Identificação da marca |
| --- | --- | --- | --- | --- |
| 212-GOV | Zilla | 2x12 | Fatboy / Eminence Governor | Documentada |
| DE-112-T75 | Engl | 1x12 | E112 / DE-112 / T-75 | Provável |
| E112-BVV | Engl | 1x12 | E112 / DE-112 / BV30V | Provável |
| E112-CV | Engl | 1x12 | E112 / DE-112 / CV | Provável |
| E112-GM | Engl | 1x12 | E112 / DE-112 / Grey Mojo | Provável |
| E112-K100 | Engl | 1x12 | E112 / DE-112 / K-100 | Provável |
| E112-P50E | Engl | 1x12 | E112 / DE-112 / P50E | Provável |
| E112-VT | Engl | 1x12 | E112 / DE-112 / V-Type | Provável |
| FB-VET30 | Zilla | 2x12 | Fatboy / WGS Veteran 30 | Documentada |
| M212-GB55 | Marshall | 2x12 | M-212 / Greenback 55 Hz | Provável |
| M212-P50 | Marshall | 2x12 | M-212 / P50 | Provável |
| M212-VINT-L | Marshall | 2x12 | M-212 / G12 Vintage, esquerdo | Provável |
| R112-V30 | Mesa Boogie | 1x12 | Rectifier / Celestion Vintage 30 Mesa specs | Documentada |

A [convenção oficial](https://overdriven.fr/overdriven/index.php/irdownloads/)
identifica FB como Zilla Fatboy. O autor identifica German-112 como Engl 112 VB
na [coleção anterior](https://overdriven.fr/overdriven/index.php/german-112-celestion-g12h55-home-studio-edition-free-guitar-cab-impulse-responses-download/);
a continuidade com as coleções atuais é uma inferência e permanece sinalizada.
Marshall é uma inferência pelo código M-212, descrição britânica vintage e readme
M. G12 Vintage; não se atribui um modelo comercial como 1936 sem documentação.
O [anúncio oficial](https://overdriven.fr/overdriven/index.php/blog-posts-2/)
de 18/10/2025 identifica R112 como Mesa Recto 1x12.

A [página de CV](https://overdriven.fr/overdriven/index.php/de-112-cv-g4-free-guitar-cab-impulse-responses-download/)
descreve um falante CV, embora seu readme repita V-Type. O catálogo preserva o texto
original e usa CV na classificação; CV e VT nunca são unificados por esse readme.
Previews continuam identificados individualmente, com microfone e prefixo próprios.
Equivalências adicionais de microfones modelados (DN, LD e RB) só são aplicadas às
coleções cujas páginas as documentam, sem propagação automática para outros packs.

Os 82 WAVs GOV são realmente mono PCM 16 bits/44,1 kHz/50 ms, apesar do readme
declarar float 32 bits. Os outros 600 são mono WAV extensível PCM 24 bits/
44,1 kHz/100 ms. Essa divergência é mantida, não corrigida no áudio.

O código M3 da coleção T75 é identificado como CND-M3 pela subpasta de origem.
Os nomes sem P de DN-409U e DN-421B têm exceções explícitas. Previews são
catalogados por arquivo, com o prefixo de captura preservado; SP1-PREVIEW não
é um microfone. LD, RB, DN e outros códigos sem modelo documentado não recebem
equivalências presumidas. Sufixos desconhecidos permanecem códigos literais.

## Distribuição

Os readmes exigem acordo escrito do proprietário para distribuir os WAVs com
produtos. O responsável pelo projeto informou autorização vitalícia para
CubeControl em 09/10/2026, ID `6a11b8c9-6ad9-48ce-994b-6cd025a4d1ae`.
O registro está em `CubeControl-1.1/Cube/docs/overdriven-distribution-agreement.txt`.
Os bloqueios específicos de publicação dos IRs foram removidos. O fluxo único
de publicação por `scripts/Build-CubeControl.ps1` continua em vigor.

A foto `micPositionsIrsNovos.png` foi usada somente para interpretar posições;
não é copiada para o produto. Sua reprodução exigiria autorização específica.
Os readmes originais acompanham os arquivos para manter atribuição e termos.

## Validação

- `python scripts/Generate-OverdrivenCatalog.py --check`
- `dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -c Release -- --overdriven-only`
- Modos `--ir-library-only`, `--ir-library-ui`, `--pack-workflow-only`,
  `--ir-audition-only` e `--localization-only` do mesmo executável.

O carregador suporta catálogo ausente/inválido sem esconder WAVs: retorna à
árvore física e informa quando os metadados estiverem indisponíveis. A distância
física é apenas metadado e nunca inicializa o controle Distance do pedal.
