# Idiomas do CubeControl

[Documentação](../../../docs/README.md) · Revisão: 10/10/2026

A opção **Idioma / Language**, no menu de reticências do cabeçalho, troca os textos imediatamente. Português (Brasil) é o idioma base; inglês está em `en-US.json`. A escolha fica separada dos presets, imagens e associações em `%LOCALAPPDATA%\CubeControl\language-v1.json`, compartilhada com a simulação. Nenhum comando MIDI é enviado ao trocar idioma. Escalas dos parâmetros e números transmitidos permanecem iguais.

## Adicionar um idioma

1. Copie `en-US.json` para um novo arquivo, por exemplo `es-ES.json`.
2. Mantenha `Version: 1`, altere `Code` para `es-ES` e `Name` para `Español`.
3. Em `Strings`, preserve as chaves em português e traduza apenas os valores. Preserve nomes próprios, números, extensões e símbolos dos atalhos físicos.
4. Coloque o arquivo na pasta `Localization` ao lado do executável e reabra o aplicativo. O idioma aparece automaticamente no submenu. Para incluí-lo nas próximas compilações, adicione-o a esta pasta no projeto.

Chaves ausentes usam os textos originais em português. Arquivos inválidos são ignorados. Não é necessário alterar o protocolo ou os ViewModels para adicionar um idioma. Mensagens de diagnóstico bruto e mensagens fornecidas pelo Windows podem permanecer no idioma de origem.

Os textos visíveis em XAML usam `loc:Translate`; bindings somente de apresentação acompanham a alteração de idioma. Campos numéricos de entrada mantêm seus bindings originais. Mensagens compostas traduzem trechos conhecidos em uma única passagem, preservando números. Diálogos são traduzidos ao abrir. O catálogo JSON é a fonte das traduções.

## Revisar textos e hints

- Mantenha as mesmas chaves em `pt-BR.json` e `en-US.json`. As chaves são os textos usados pelo código; revise os valores para melhorar a redação sem alterar os identificadores.
- Coloque todas as traduções dentro de `Strings`. Na raiz, use apenas `Version`, `Code`, `Name` e `Strings`; textos fora de `Strings` são ignorados pelo carregador.
- Em português, use **gabinete**, **forma de onda**, **faixa** e **metrônomo**. Preserve nomes de equipamentos, extensões, siglas e termos musicais estabelecidos, como preset, IR, chorus e backing track. Em inglês, use grafia dos EUA.
- Descreva a ação e seu resultado. Distinga edição no computador, prévia temporária em RAM e gravação no pedal. Quando a recuperação existir apenas na memória da sessão, informe esse limite; não prometa um arquivo de backup.
- Confira os hints de mouse e teclado no controle atual. Preserve números, unidades, placeholders como `{0}` e nomes dos comandos citados nas instruções.
- Use `loc:Translate` também em tooltips e textos de acessibilidade. Nomes de arquivos e dispositivos fornecidos pelo usuário ou pelo Windows devem ser preservados.

Validação direcionada, a partir da **raiz do repositório**:

```powershell
dotnet run --project .\CubeControl-1.1\Cube\windows\Cube.Desktop.Tests -c Release -- --localization-only
```

Captura opcional de interface sem conexão automática, usando o pacote gerado pelo fluxo único, também a partir da raiz:

```powershell
.\scripts\Build-CubeControl.ps1
& 'C:\CubeControl-Build\publish\CubeControl-1.1.0\CubeControl.exe' --smoke C:/CubeControl-Build/reports/language-en.png --english --offline --gallery
```

Esse modo abre a interface para gerar a captura e não persiste a escolha de idioma. Implementação: [LanguageService.cs](LanguageService.cs). Os demais modos estão no [guia Windows](../../README.md).
