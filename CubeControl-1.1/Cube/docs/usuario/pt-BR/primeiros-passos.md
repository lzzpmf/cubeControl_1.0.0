# Primeiros passos

## Instalar e abrir
Extraia o pacote completo para uma pasta do computador e abra `CubeControl.exe`. Mantenha as pastas do pacote junto ao executável. Não é necessário instalar o SDK .NET para usar o pacote publicado.

## Conectar o Cube Baby
1. Ligue o Cube Baby de guitarra e conecte um cabo USB de dados ao computador.
2. Feche outros editores que estejam usando a porta MIDI.
3. Abra o cubeControl e aguarde a identificação e a leitura de A/B/C e dos oito slots de IR.
4. Se necessário, use o botão de conexão e consulte os detalhes em Dispositivo e ajuda.

O editor usa USB-MIDI, não Bluetooth. Não há perfil implementado para Cube Baby Bass, AC, Chocolate ou Blackbox. Desconectar manualmente pausa a reconexão automática; conectar novamente a retoma.

## Fazer o primeiro ajuste
Selecione no aplicativo o mesmo A/B/C que você escolheu fisicamente no pedal. Abra um módulo da Pedaleira e ajuste um controle. Selecionar A/B/C no aplicativo escolhe o destino da edição, mas não aciona o footswitch. Leia [Presets e arquivos](presets.md) antes de salvar.

## Personalizar a leitura
Tema, idioma e zoom ficam no menu de reticências. A central de documentação funciona offline e sem pedal. Guias acompanham o idioma do aplicativo; PDFs indicam seu próprio idioma.

O zoom vai de 50% a 200%, em passos de 10%. Use Ctrl +, Ctrl − e Ctrl 0 para aumentar, diminuir e restaurar 100%; isso não altera o DPI do Windows.

Se a conexão falhar, consulte [Problemas comuns](problemas.md).

## Encontrar ajuda
Em **Dispositivo e ajuda**, **Manual do cubeControl** abre o manual HTML no navegador padrão, no idioma e tema atuais do aplicativo. Ele inclui busca no texto completo, índice por capítulo, impressão e troca entre português e inglês. Também é possível abrir `Assets/Documents/manual/index.html` diretamente, sem internet. A seleção de tema do navegador não altera a aparência do aplicativo.

A busca da Central de Documentação procura no título, resumo e palavras-chave do catálogo, não no texto integral dos guias. Todos os termos separados por espaço precisam corresponder; maiúsculas e acentos são ignorados. Se não houver versão no idioma selecionado, o app usa português brasileiro ou, na ausência dele, a primeira versão disponível. Os manuais preservados descrevem suas versões de origem; para os fluxos atuais do aplicativo, use os guias da Central.

## Navegar e consultar ajuda
Com o cursor sobre a barra principal, a roda do mouse avança ou retorna entre as cinco abas e para nos extremos. Fora da barra, a roda continua atuando sobre a página ou o controle. Use Tab e Shift+Tab para navegar pelo teclado; Alt sozinho não ativa marcações de navegação. As dicas aparecem após 1,2 segundo e desaparecem cinco segundos depois de exibidas; um novo hover permite exibi-las novamente. A janela de documentação acompanha o tema do app, com zoom aplicado ao conteúdo de leitura.
