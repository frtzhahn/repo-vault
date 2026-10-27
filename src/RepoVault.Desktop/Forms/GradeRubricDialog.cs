using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class GradeRubricDialog : Form
{
    private readonly AppSession _session;
    private readonly int _repoId;
    private readonly string _repoTitle;
    private readonly string _repoSlug;
    private readonly string _courseCode;
    private readonly string _studentOrTeam;
    private readonly int _sourceLines;
    private readonly int _commentLines;
    private readonly int _totalFiles;

    public GradeRubricDialog(
        AppSession session, int repoId, string repoTitle, string repoSlug,
        string courseCode, string studentOrTeam,
        int sourceLines = 0, int commentLines = 0, int totalFiles = 0)
    {
        _session = session;
        _repoId = repoId;
        _repoTitle = repoTitle;
        _repoSlug = repoSlug;
        _courseCode = courseCode;
        _studentOrTeam = studentOrTeam;
        _sourceLines = sourceLines;
        _commentLines = commentLines;
        _totalFiles = totalFiles;
        InitializeComponent();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        lblHeader.Text = $"Grade Repository - {_repoSlug}";
        lblCourseAndTeam.Text = $"Course: {_courseCode}  |  Student/Team: {_studentOrTeam}";
        lblMetrics.Text = $"Code Metrics: {_sourceLines:N0} Source LOC  |  {_commentLines:N0} Comments  |  {_totalFiles} Files";
        Text = $"Grade Repository - {_repoSlug}";

        await LoadExistingEvaluationAsync();
    }

    private async Task LoadExistingEvaluationAsync()
    {
        try
        {
            EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(_repoId);
            if (eval is not null)
            {
                nudArch.Value = Math.Clamp(eval.ArchitectureScore, 0, 25);
                nudFunc.Value = Math.Clamp(eval.FunctionalityScore, 0, 25);
                nudQual.Value = Math.Clamp(eval.CodeQualityScore, 0, 25);
                nudDoc.Value = Math.Clamp(eval.DocumentationScore, 0, 25);
                txtNotes.Text = eval.CritiqueNotes ?? string.Empty;
            }

            UpdateTotalScore();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load existing rubric: " + ex.Message, "Grade Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ScoreInput_ValueChanged(object? sender, EventArgs e)
    {
        UpdateTotalScore();
    }

    private void UpdateTotalScore()
    {
        int total = (int)(nudArch.Value + nudFunc.Value + nudQual.Value + nudDoc.Value);
        lblTotalScore.Text = $"Total Score: {total} / 100";
    }

    private async void btnSave_Click(object? sender, EventArgs e)
    {
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            MessageBox.Show(this, "you must be logged in to grade submissions.", "Grade Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int arch = (int)nudArch.Value;
        int func = (int)nudFunc.Value;
        int qual = (int)nudQual.Value;
        int doc = (int)nudDoc.Value;
        string notes = txtNotes.Text.Trim();
        bool lockRepo = chkLockRepo.Checked;

        btnSave.Enabled = false;
        try
        {
            var (ok, message, _) = await _session.Evaluations.SaveEvaluationAsync(
                actor, _repoId, arch, func, qual, doc, notes, lockRepo);

            if (!ok)
            {
                MessageBox.Show(this, message, "Grade Repository", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "rubric evaluation saved successfully.", "Grade Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not save evaluation: " + ex.Message, "Grade Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }
}
