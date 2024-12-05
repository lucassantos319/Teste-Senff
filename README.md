# Teste-Senff

## Objetivo do teste
Desenvolver um pacote NuGet para mensageria utilizando .NET Core que abstraia a integração com o RabbitMQ.


## Requisitos Técnicos
### Pacote NuGet
Criar um pacote que permita a integração com o RabbitMQ de forma abstrata e facilite o uso por outros desenvolvedores;

### Manejo de retentativas
Implementar uma lógica de retentativas automáticas para falhas durante a publicação ou consumo de mensagens com garantia de confiabilidade no tratamento de falhas.

### Aplicação de Teste
Desenvolver uma aplicação de teste que utilize o pacote criado, demonstrando a funcionalidade de publicação e consumo de mensagens.

### Docker Compose
Configurar um ambiente com Docker Compose para incluir toda a infraestrutura necessária para a execução do teste, incluindo o RabbitMQ e a aplicação de teste.



## Requisitos Funcionais
### Interface de Mensageria
Implementar uma interface clara e amigável para publicação e consumo de mensagens, de modo que outros desenvolvedores não precisem se preocupar com detalhes de implementação

## Configuração do RabbitMQ
Preparar o pacote para realizar as operações de forma segura, com opção de configuração para ajuste dos parâmetros de conexão e retentativas.

## Documentação Básica
Incluir uma documentação básica do pacote com instruções de uso, descrevendo as funcionalidades e exemplos de código para facilitar a integração na aplicação de teste.
