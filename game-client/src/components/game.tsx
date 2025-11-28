import { useEffect, useRef } from "react";

const Game = () => {
  const iframeRef = useRef<HTMLIFrameElement>(null);

  const compileUrl = "build-web/index.html";

  useEffect(() => {
    if (iframeRef.current) {
      iframeRef.current.focus();
    }
  }, []);

  return (
    <div className="relative bg-card border border-border/50 rounded-lg overflow-hidden shadow-lg aspect-video">
      <iframe
        ref={iframeRef}
        src={compileUrl}
        title="content external compiler"
        width="100%"
        height="100%"
        tabIndex={0}
        className="w-full h-full border-0"
        allowFullScreen
      ></iframe>
    </div>
  );
};

export default Game;
