﻿module Foundation.Test.ViewModels {

    @Core.SecureFormViewModelDependency({ name: "AngularTranslateFormViewModel", templateUrl: "|Foundation|/Foundation.Test.HtmlClient/views/tests/angularTranslateview.html" })
    export class AngularTranslateFormViewModel extends ViewModel.ViewModels.FormViewModel {

        public constructor( @Core.Inject("$translate") public $translate: ng.translate.ITranslateService) {
            super();
        }

        public text: string = "KnownError";

        @ViewModel.Command()
        public changeText(): void {
            this.text = this.$translate.instant("UnKnownError");
        }

        @ViewModel.Command()
        public changeLanguage(): void {
            this.$translate.use("FaIr");
        }
    }
}