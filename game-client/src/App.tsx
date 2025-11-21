// import './App.css'

import { useEffect, useRef } from "react"

function App() {
  const iframeRef = useRef<HTMLIFrameElement>(null);

  const compileUrl = 'build-web/index.html'
  
  useEffect(() => {
    if (iframeRef.current) {
      iframeRef.current.focus()
    }
  }, [])
  
  return (
    <div style={{ width: '100%', height: '600px', margin: '20px 0', border: '1px solid red'}}>
      <iframe
        ref={iframeRef}
        src={compileUrl}
        title='content external compiler'
        width="100%"
        height="100%"
        tabIndex={0}
      allowFullScreen
      ></iframe>
    </div>
  )
}

export default App
