const { useState, useEffect } = React;

function PredictionsApp() {
    const [matches, setMatches] = useState([]);
    const [loading, setLoading] = useState(true);
    const [savingId, setSavingId] = useState(null);
    const [savedId, setSavedId] = useState(null);

    // Charger les matchs à venir au démarrage
    useEffect(() => {
        fetch('/Predictions/Upcoming')
            .then(res => res.json())
            .then(data => {
                // On initialise les champs de saisie avec le prono existant (ou vide)
                const withInputs = data.map(m => ({
                    ...m,
                    inputA: m.prediction ? m.prediction.predictedScoreA : '',
                    inputB: m.prediction ? m.prediction.predictedScoreB : ''
                }));
                setMatches(withInputs);
                setLoading(false);
            });
    }, []);

    // Mettre à jour un champ de score localement
    function handleChange(id, side, value) {
        setMatches(matches.map(m =>
            m.id === id ? { ...m, [side]: value } : m
        ));
    }

    // Envoyer un prono au serveur
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
        return d.toLocaleString('fr-FR', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    if (loading) {
        return <p>Chargement des matchs…</p>;
    }

    if (matches.length === 0) {
        return <p>Aucun match à venir pour le moment.</p>;
    }

    return (
        <div>
            {matches.map(match => {
                const isValid = match.inputA !== '' && match.inputB !== '';
                return (
                    <div key={match.id} className="card mb-3">
                        <div className="card-body">
                            <div className="text-muted mb-2">{formatDate(match.kickoff)}</div>
                            <div className="d-flex align-items-center gap-2 flex-wrap">
                                <strong style={{ minWidth: '90px' }}>{match.teamA}</strong>
                                <input
                                    type="number" min="0" className="form-control"
                                    style={{ width: '70px' }}
                                    value={match.inputA}
                                    onChange={e => handleChange(match.id, 'inputA', e.target.value)}
                                />
                                <span>–</span>
                                <input
                                    type="number" min="0" className="form-control"
                                    style={{ width: '70px' }}
                                    value={match.inputB}
                                    onChange={e => handleChange(match.id, 'inputB', e.target.value)}
                                />
                                <strong style={{ minWidth: '90px' }}>{match.teamB}</strong>
                                <button
                                    className="btn btn-primary"
                                    disabled={!isValid || savingId === match.id}
                                    onClick={() => handleSave(match)}
                                >
                                    {savingId === match.id ? 'Enregistrement…' : 'Valider'}
                                </button>
                                {savedId === match.id && (
                                    <span className="text-success">✓ Enregistré</span>
                                )}
                            </div>
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

ReactDOM.createRoot(document.getElementById('predictions-root')).render(<PredictionsApp />);