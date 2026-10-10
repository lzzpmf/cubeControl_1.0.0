# Biblioteca Overdriven.fr

A biblioteca contém 13 coleções e 682 WAVs, preservados byte a byte em
`windows/Cube.Desktop/Assets/ImpulseResponses/Overdriven.fr`. O manifesto
`Overdriven.catalog.v1.json` é independente dos packs de oito slots.

## Atualização reproduzível

Na raiz do workspace, execute `python scripts/Generate-OverdrivenCatalog.py`.
Use `--check` para conferir catálogo, WAVs e readmes sem escrever arquivos.
A ferramenta usa a documentação em `irsNovos`, exceções explícitas de nomes
e cabeçalhos WAV. Não baixa arquivos nem processa áudio.

O manifesto v1 registra caminho relativo à biblioteca, SHA-256, coleção,
caixa/falante descritos no readme, potência/pré de microfone, código/modelo de
microfone, posição, distância aproximada em mm, variantes, prefixo de captura,
preview, nome original e formato técnico real. A identidade da coleção é o
nome original da pasta; CV e VT permanecem distintas. Não se deve renomear os
caminhos após criar packs que os referenciem.

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
