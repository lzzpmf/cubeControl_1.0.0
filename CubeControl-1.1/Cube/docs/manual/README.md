# Manual HTML local

Abra `index.html` ou use **Dispositivo e ajuda → Manual do cubeControl** no aplicativo. O site inteiro funciona por `file://`, sem servidor, instalação ou conexão de rede. Português e inglês têm os mesmos capítulos. A busca consulta o texto integral; os guias e PDFs da central nativa continuam disponíveis.

## Atualizar o conteúdo

Os procedimentos vêm de `../usuario/pt-BR` e `../usuario/en-US`; títulos, resumos e palavras-chave vêm de `../usuario/catalogo.v1.json`. Corrija os dois idiomas na origem. Os roteiros introdutórios, glossário e composição das páginas estão em `scripts/Build-Manual.mjs`, na raiz do repositório. Estilos e comportamento ficam em `assets/`.

Após uma alteração de conteúdo, execute na raiz:

```powershell
node scripts/Build-Manual.mjs
node scripts/Validate-Manual.mjs
./scripts/Validate-Documentation.ps1
```

O gerador usa apenas a biblioteca padrão do Node.js; o usuário final e o build .NET não precisam de Node.js. As páginas e o índice gerados são versionados. `sources.json` registra hashes das fontes e a versão; a validação do pacote recusa conteúdo desatualizado. Não edite os HTMLs gerados diretamente.

O projeto desktop copia esta pasta para `Assets/Documents/manual`. `references/` e a imagem do pedal são cópias geradas dos recursos do aplicativo, para permitir também distribuir esta pasta sozinha. A arte do pedal é ilustrativa, não uma fotografia. Inter é distribuída localmente sob a licença em `assets/Inter-LICENSE.txt`, obtida do projeto https://github.com/rsms/inter. Não há fontes, scripts, telemetria ou serviços externos carregados pelo manual.

Antes de publicar, confira abertura direta, ambos os idiomas, busca, links, impressão, temas, teclado, largura de 390 px e zoom de 200%. O navegador recebe o idioma e o tema do app; a preferência de tema do manual é independente e pode ser alterada no cabeçalho. Sem JavaScript, todo o conteúdo e a navegação continuam disponíveis; busca e seleção de tema requerem JavaScript.
