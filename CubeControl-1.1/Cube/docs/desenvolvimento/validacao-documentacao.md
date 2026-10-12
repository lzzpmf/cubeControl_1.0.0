# Validação da central de documentação

Data: 08/10/2026. [Índice](../README.md).

## Como repetir a validação

Na raiz do repositório, execute as verificações de fontes (Python 3 e PowerShell):

```powershell
python scripts/qa/validate_document_links.py
.\scripts\Validate-Documentation.ps1
```

O primeiro comando confere destinos e âncoras locais do acervo Markdown mantido; não consulta URLs externas. O segundo confere o catálogo e os arquivos destinados à central, incluindo idiomas, links publicados e cabeçalhos PDF. Eles não verificam a correção das afirmações em relação ao código nem o layout visual.

Para um pacote já construído, passe a **raiz do pacote**, que contém `Assets`, ao validador de hashes:

```powershell
.\scripts\Validate-Documentation.ps1 -PackageRoot 'C:\CubeControl-Build\publish\CubeControl-1.1.0'
```

Para os testes WPF, siga os [requisitos de desenvolvimento Windows](../../windows/README.md#requisitos-e-versão). O argumento `--documentation-root` recebe a **pasta de documentos**, diferente de `-PackageRoot`:

```powershell
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -c Release -- --documentation-only
dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -c Release -- --documentation-only --documentation-root=C:\CubeControl-Build\publish\CubeControl-1.1.0\Assets\Documents
```

O último comando executa o código de testes do checkout sobre os documentos do pacote; não executa o aplicativo instalado. Use `--no-build` apenas quando o binário de testes estiver atualizado. Compare hashes novamente após editar as fontes: um pacote anterior pode estar correto para sua versão e divergente do checkout atual.

## Resultado histórico de 08/10/2026

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
