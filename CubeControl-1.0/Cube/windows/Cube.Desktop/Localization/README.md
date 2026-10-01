# Idiomas do CubeControl

O botão **Idioma / Language** troca os textos imediatamente. Português (Brasil) é o idioma base; inglês está em `en-US.json`. A escolha fica separada dos presets, imagens e associações em `%LOCALAPPDATA%\CubeControl\language-v1.json`. Nenhum comando MIDI é enviado ao trocar idioma. Escalas dos parâmetros e números transmitidos permanecem iguais.

## Adicionar um idioma

1. Copie `en-US.json` para um novo arquivo, por exemplo `es-ES.json`.
2. Mantenha `Version: 1`, altere `Code` para `es-ES` e `Name` para `Español`.
3. Em `Strings`, preserve as chaves em português e traduza apenas os valores. Preserve nomes próprios, números, extensões e símbolos dos atalhos físicos.
4. Coloque o arquivo na pasta `Localization` ao lado do executável e reabra o aplicativo. O idioma aparece automaticamente no botão. Para incluí-lo nas próximas compilações, adicione-o a esta pasta no projeto.

Chaves ausentes usam os textos originais em português. Arquivos inválidos são ignorados. Não é necessário alterar o protocolo ou os ViewModels para adicionar um idioma. Mensagens de diagnóstico bruto e mensagens fornecidas pelo Windows podem permanecer no idioma de origem.

Os textos visíveis em XAML usam `loc:Translate`; bindings somente de apresentação acompanham a alteração de idioma. Campos numéricos de entrada mantêm seus bindings originais. Mensagens compostas traduzem trechos conhecidos em uma única passagem, preservando números. Diálogos são traduzidos ao abrir. O catálogo JSON é a fonte das traduções.

Validação direcionada: `dotnet run --project windows/Cube.Desktop.Tests -- --localization-only`. Smoke sem dispositivo: `dotnet run --project windows/Cube.Desktop -- --smoke windows/artifacts/language-en.png --english --offline --gallery`. O modo de smoke não persiste a escolha de idioma.
