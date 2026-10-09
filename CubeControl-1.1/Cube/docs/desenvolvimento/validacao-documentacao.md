# Validação da central de documentação

Data: 08/10/2026. [Índice](../README.md).

## Resultado

- 15 guias com versões pt-BR e en-US; três PDFs cadastrados. Catálogo e conteúdo somam 34 arquivos publicados. As fontes dos guias permanecem somente em `docs/usuario`.
- Os PDFs oficiais/adaptados foram preservados por comparação de SHA-256. O manual oficial foi conferido visualmente; as versões adaptadas identificam seu idioma e sua origem na primeira página.
- `python scripts/qa/validate_document_links.py`: 499 links locais válidos, incluindo âncoras, no acervo mantido. Documentação de terceiros dentro do APK permanece fora dessa revisão.
- `scripts/Validate-Documentation.ps1`: valida versões, categorias, idiomas, caminhos, PDFs, links e âncoras publicados. Com `-PackageRoot`, compara cada arquivo final com a fonte por hash.
- `--documentation-only`: 143 verificações de catálogo, idiomas, busca, links, Markdown, seleção de texto, falhas de arquivo/PDF, recuperação de erros e layout. Executado também com `--documentation-root=C:\CubeControl-Build\publish\CubeControl-1.1.0\Assets\Documents`, lendo o conteúdo instalado.
- `--localization-only`: 13 verificações aprovadas. `--assets-only`: carregamento dos recursos e documentos aprovado.
- Builds oficiais normal e simulado concluídos, incluindo conferência dos documentos finais. Foi necessário corrigir nomes acentuados corrompidos nas referências de imagens do projeto para que a verificação de assets concluísse.

## Inspeção visual e alcance

Foram geradas 18 capturas combinando pt-BR/en-US, Claro/Escuro/Alto contraste e janelas ampla/compacta com zoom 100%/200%. As capturas estão na saída externa `C:/CubeControl-Build/documentation-qa`. A inspeção incluiu tabelas, lista, busca, sumário e leitura compacta; o cabeçalho é recolhido durante a leitura compacta para priorizar o documento. Teclado/foco e seleção de texto foram conferidos por controles WPF.

Os testes não abrem leitores externos nem usam rede ou pedal. A falha de associação de PDF é simulada. A inspeção de alto contraste usa os recursos do tema WPF; não constitui uma avaliação com Narrator ou uma homologação de acessibilidade do Windows.
