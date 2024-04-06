namespace pages {

    export class login extends Bootstrap {

        get appName(): string {
            return "login";
        }

        protected init(): void {

        }

        public login_onclick() {
            let account_data = {
                email: $ts.value("#email"),
                passwd: $ts.value("#password")
            }

            if (Strings.Empty(account_data.email)) {
                return;
            } else if (Strings.Empty(account_data.passwd)) {
                return;
            } else {
                account_data.passwd = md5(account_data.passwd);
            }

            $ts.post("/access/login", account_data, function (result) {
                if (result.code == 0) {
                    $goto("/gallery");
                } else {

                }
            });
        }
    }
}