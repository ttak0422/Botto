let 
  sources = import ./nix/sources.nix;
  pkgs = import sources.nixpkgs {};
in pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnet
    nodejs 
    nodePackages.node2nix
    # keep this line if you use bash
    bashInteractive
  ];
}