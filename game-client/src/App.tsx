import Navbar from "./components/navbar"
import Game from "./components/game"
import { Footer } from "./components/footer"
import { ControlsSection } from "./components/controls-section"
import { AboutSection } from "./components/about-section"
import { ChangelogSection } from "./components/changelog-section"
import { CreditsSection } from "./components/credits-section"

function App() {
  return (
    <div className="min-h-screen bg-[#0a0a0a] flex flex-col">
      <Navbar />
      <main className="flex-1 px-6 py-12">
        <div className="w-full max-w-6xl mx-auto">
          <Game />
          <ControlsSection />
          <AboutSection />
          {!import.meta.env.DEV && <ChangelogSection />}
          <CreditsSection />
        </div>
      </main>
      <Footer />
    </div>
  )
}

export default App
