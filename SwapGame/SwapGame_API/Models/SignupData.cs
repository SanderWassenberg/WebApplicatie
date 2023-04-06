namespace SwapGame_API.Models {
    public struct SignupData {
        public string Name { get; set; }
        public string Password { get; set; }

        // ! Must update is_valid when adding new fields !

        public bool complete() => 
            this.Name is not null && 
            this.Password is not null;
    }
}
