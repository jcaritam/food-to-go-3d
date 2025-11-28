import Navbar from "./components/navbar";
import Game from "./components/game";
import { Footer } from "./components/footer";

function App() {
  return (
    <div className="min-h-screen bg-[#0a0a0a] flex flex-col">
      <Navbar />
      <main className="flex-1 flex items-center justify-center px-6 py-12">
        <div className="w-full max-w-6xl">
          <Game />

          <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-card border border-border/30 rounded-lg p-4">
              <div className="text-xs text-muted-foreground mb-2 uppercase tracking-wider">
                Controles
              </div>
              <p className="text-sm text-foreground">
                W-A-S-D para movimiento, Espacio para interactuar
              </p>
            </div>
            <div className="bg-card border border-border/30 rounded-lg p-4">
              <div className="text-xs text-muted-foreground mb-2 uppercase tracking-wider">
                Objetivo
              </div>
              <p className="text-sm text-foreground">Prepara y sirve platos</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}

export default App;
