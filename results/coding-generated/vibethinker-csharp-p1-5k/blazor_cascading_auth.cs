public class UserDashboard
{
            [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }

            [Inject] publicIOrderService UserService { get; set; }

            public string? DisplayName { get; set; }
            public bool IsAuthenticated { get; set; }

            async void OnInitializedAsync()
            {
                await AuthState;
                var authState = await AuthState;

                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    IsAuthenticated = true;
                    var nameClaim = authState.User.Identity.NameClaim;
                    var name = await UserService.GetDisplayNameAsync(nameClaim);
                    DisplayName = name;
                }
                else
                {
                    IsAuthenticated = false;
                    DisplayName = "Guest";
                }
            }

        }