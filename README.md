# Projeto1_IF

Sistema ASP.NET Core MVC com Identity e EF Core para o trabalho final de Programação Web II.

## Funcionalidades

- Autocadastro de Médico e Nutricionista com login, senha e dados do profissional na mesma tela.
- Perfis com roles: `Medico`, `Nutricionista`, `GerenteMedico`, `GerenteNutricionista` e `GerenteGeral`.
- Cadastro, edição, detalhes e exclusão de pacientes vinculados ao profissional logado.
- Controle de acesso por role no controller, com restrição de visualização por usuário.
- Seed automático de roles, planos e usuários gerentes na inicialização.

## Banco de Dados

- O projeto usa a connection string `DefaultConnection` em `appsettings.json`.
- O seed de planos roda quando `tbPlano` estiver vazia.
- Os usuários gerentes são criados automaticamente com os emails abaixo:
	- `gerente.medico@example.com`
	- `gerente.nutri@example.com`
	- `gerente.geral@example.com`
- Senha padrão dos gerentes: `Gerente@123`

## Executar

No Windows, abra a solução no Visual Studio, confira a connection string `DefaultConnection` em `appsettings.json` e execute o projeto com `F5` ou `Ctrl+F5`.

Depois, abra o endereço que aparecer no navegador.

## Observações

- As views de paciente estão em `Views/TbPaciente` e são acessadas pelo controller `TbPacientesController`.
- O projeto inclui `Areas/Identity/Pages/Account/RegisterMedico` e `RegisterNutricionista` como páginas próprias de autocadastro.

## Autor

- **Nome**: Tadeu dos Santos Jerônimo
- **Matrícula**: 2026202194
- **E-mail**: tadeus.jeronimo@gmail.com
- **Disciplina**: Programação Web II - IF Sudeste/MG