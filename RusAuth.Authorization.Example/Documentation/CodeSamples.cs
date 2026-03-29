namespace RusAuth.Authorization.Example.Documentation;

public static class CodeSamples
{

    public static string ProgramRegistration => """
                                                using RusAuth.Authorization.Contracts.Rest;
                                                using RusAuth.Authorization.Extensions;

                                                builder.Services.AddRusAuthConfirmationClient(new RusAuthOptions
                                                {
                                                    BaseUrl = builder.Configuration["RusAuth:BaseUrl"]!,
                                                    Token = builder.Configuration["RusAuth:Token"]!,
                                                    TimeOut = 15
                                                });
                                                """;

    public static string CallToConfirmFlow => """
                                              var response = await rusAuthClient.CallToConfirmAsync(new RusAuthCallToConfirmRequest
                                              {
                                                  PhoneNumber = "+79991234567",
                                                  ExpirationMinute = 10,
                                                  WebHook = "https://client.example.com/api/rusauth/callback/confirmation",
                                                  WebHookBearerToken = "client-callback-bearer-token"
                                              }, cancellationToken);
                                              """;

    public static string CheckConfirmationFlow => """
                                                  var status = await rusAuthClient.CheckConfirmationAsync(new RusAuthCheckConfirmationRequest
                                                  {
                                                      PhoneNumber = "+79991234567",
                                                      TransactionId = response.TransactionId
                                                  }, cancellationToken);
                                                  """;

    public static string WebHookContract => """
                                            POST /api/rusauth/callback/confirmation
                                            Authorization: Bearer <ваш локальный callback bearer>

                                            {
                                              "transactionId": "f3d2d9d0d8cf4d8e8fbb0e1090f7d245",
                                              "clientPhoneNumber": "+79991234567"
                                            }
                                            """;

    public static string ClientResponsibility => """
                                                 1. Сохранить TransactionId и номер клиента в своей системе.
                                                 2. Дождаться вебхук или выполнить ручную проверку через CheckConfirmation.
                                                 3. После статуса Success применить свои правила входа, подтверждения операции или подтверждения действия.
                                                 4. Не считать РосАвт своей SSO-системой: РосАвт даёт только механизм подтверждения звонком.
                                                 """;
    public static string BuildAppSettings(string baseUrl) => $$"""
                                                               {
                                                                 "RusAuth": {
                                                                   "BaseUrl": "{{baseUrl}}",
                                                                   "Token": "ВАШ_ТОКЕН_КОМПАНИИ",
                                                                   "TimeOut": 15
                                                                 },
                                                                 "Example": {
                                                                   "CallbackBearerToken": "ВАШ_ЛОКАЛЬНЫЙ_CALLBACK_BEARER"
                                                                 }
                                                               }
                                                               """;
}