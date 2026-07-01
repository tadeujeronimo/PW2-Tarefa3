// Tadeu dos Santos Jerônimo
// jquery.validate.pt-br.js - Localização para Português do Brasil

$.validator.setDefaults({
    messages: {
        required: "Este campo é obrigatório.",
        remote: "Por favor, corrija este campo.",
        email: "Por favor, insira um endereço de e-mail válido.",
        url: "Por favor, insira uma URL válida.",
        date: "Por favor, insira uma data válida.",
        dateISO: "Por favor, insira uma data válida (ISO).",
        number: "Por favor, insira um número válido.",
        digits: "Por favor, insira somente dígitos.",
        creditcard: "Por favor, insira um número de cartão de crédito válido.",
        equalTo: "Por favor, insira o mesmo valor novamente.",
        maxlength: $.validator.format("Por favor, insira não mais do que {0} caracteres."),
        minlength: $.validator.format("Por favor, insira pelo menos {0} caracteres."),
        rangelength: $.validator.format("Por favor, insira um valor entre {0} e {1} caracteres."),
        range: $.validator.format("Por favor, insira um valor entre {0} e {1}."),
        max: $.validator.format("Por favor, insira um valor menor ou igual a {0}."),
        min: $.validator.format("Por favor, insira um valor maior ou igual a {0}.")
    }
});

$.validator.methods.date = function (value, element) {
    // Aceita formato dd/mm/yyyy
    return this.optional(element) || /^\d{2}\/\d{2}\/\d{4}$/.test(value);
};

$.validator.methods.number = function (value, element) {
    // Aceita vírgula como separador decimal (padrão BR)
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)?(?:,\d+)?$/.test(value);
};