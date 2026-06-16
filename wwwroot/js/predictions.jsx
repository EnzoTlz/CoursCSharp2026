const { useState, useEffect } = React;

function PredictionsApp() {
    const [matches, setMatches] = useState([]);
    const [loading, setLoading] = useState(true);
    const [savingId, setSavingId] = useState(null);
    const [savedId, setSavedId] = useState(null);

    useEffect(() => {
        fetch('/Predictions/Upcoming')
            .then(res => res.json())
            .then(data => {
                const withInputs = data.map(m => ({
                    ...m,
                    inputA: m.prediction ? m.prediction.predictedScoreA : '',
                    inputB: m.prediction ? m.prediction.predictedScoreB : ''
                }));
                setMatches(withInputs);
                setLoading(false);
            });
    }, []);

    function handleChange(id, side, value) {
        setMatches(matches.map(m =>
            m.id === id ? { ...m, [side]: value } : m
        ));
    }

    function handleSave(match) {
        setSavingId(match.id);
        setSavedId(null);

        fetch('/Predictions/Save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                matchId: match.id,
                scoreA: parseInt(match.inputA, 10),
                scoreB: parseInt(match.inputB, 10)
            })
        }).then(res => {
            setSavingId(null);
            if (res.ok) {
                setSavedId(match.id);
                setTimeout(() => setSavedId(null), 2000);
            } else {
                alert("Erreur lors de l'enregistrement du pronostic.");
            }
        });
    }

    function formatDate(iso) {
        const d = new Date(iso);
        return {
            day: d.toLocaleDateString('fr-FR', { day: '2-digit', month: '2-digit' }),
            time: d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
        };
    }

    if (loading) {
        return <div className="pred-loading">Chargement des matchs…</div>;
    }

    if (matches.length === 0) {
        return (
            <div className="empty-state">
                <p>Aucun match à venir pour le moment. Reviens bientôt !</p>
            </div>
        );
    }

    return (
        <div className="pred-list">
            {matches.map(match => {
                const isValid = match.inputA !== '' && match.inputB !== '';
                const hasPrediction = match.prediction !== null;
                const date = formatDate(match.kickoff);
                return (
                    <div key={match.id} className="pred-card">
                        <div className="pred-date">
                            <span className="match-day">{date.day}</span>
                            <span className="match-time">{date.time}</span>
                        </div>
                        <div className="pred-team pred-team-a">{match.teamA}</div>
                        <div className="pred-inputs">
                            <input
                                type="number" min="0" className="form-control pred-score"
                                value={match.inputA}
                                onChange={e => handleChange(match.id, 'inputA', e.target.value)}
                            />
                            <span className="pred-sep">:</span>
                            <input
                                type="number" min="0" className="form-control pred-score"
                                value={match.inputB}
                                onChange={e => handleChange(match.id, 'inputB', e.target.value)}
                            />
                        </div>
                        <div className="pred-team pred-team-b">{match.teamB}</div>
                        <div className="pred-action">
                            <button
                                className="btn btn-gold btn-sm"
                                disabled={!isValid || savingId === match.id}
                                onClick={() => handleSave(match)}
                            >
                                {savingId === match.id ? '…' : (hasPrediction ? 'Modifier' : 'Valider')}
                            </button>
                            {savedId === match.id && (
                                <span className="pred-saved">✓ Enregistré</span>
                            )}
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

ReactDOM.createRoot(document.getElementById('predictions-root')).render(<PredictionsApp />);