import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import './style.css';

function App() {
  const [status, setStatus] = useState('Łączenie z serwerem…');
  const [connected, setConnected] = useState(false);
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    const controller = new AbortController();
    setStatus('Łączenie z serwerem…');
    setConnected(false);
    fetch('/api/status', { signal: controller.signal })
      .then(response => { if (!response.ok) throw new Error(); return response.json(); })
      .then(data => { setStatus(data.status); setConnected(true); })
      .catch(error => { if (error.name !== 'AbortError') setStatus('Nie udało się połączyć z serwerem. Spróbuj ponownie.'); });
    return () => controller.abort();
  }, [attempt]);
  return <main><header><span className="mark" aria-hidden="true">W</span><strong>WebStore</strong></header>
    <section><p className="eyebrow">TWÓJ SKLEP W JEDNYM MIEJSCU</p><h1>Witaj w WebStore.</h1>
      <p className="intro">Miejsce na Twoje produkty, zamówienia i relacje z klientami.</p>
      <div className="status" role="status"><span className={connected ? 'dot connected' : 'dot'} />{status}</div>
      {!connected && <button onClick={() => setAttempt(value => value + 1)}>Połącz ponownie</button>}
    </section><footer>WebStore</footer></main>;
}
createRoot(document.getElementById('root')).render(<App />);
