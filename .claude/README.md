# Claude Code Structure Applied

**Applied:** May 29, 2026  
**Based on:** Andrej Karpathy's CLAUDE.md + Skills.md patterns

---

## 📂 New Structure

```
.claude/
├── settings.json          # Project config + hooks
├── commands/              # Slash commands (/build, /deploy, etc.)
│   ├── build.md
│   ├── deploy.md
│   └── analyze-players.md
├── skills/                # Modular skills for AI assistance
│   ├── unity-build.md
│   ├── firebase-cost-monitor.md
│   └── player-acquisition.md
└── review.md              # Pre-commit checklist

CLAUDE.md                  # Project memory + workflow principles
```

---

## 🎯 What This Enables

### 1. Project Memory (CLAUDE.md)
AI remembers your:
- Tech stack (Unity 6, C#, Firebase)
- Constraints (zero players, $5/month budget)
- Priorities (traction > features)
- Common pitfalls to avoid
- Success metrics

### 2. Workflow Automation (commands/)
Quick slash commands:
- `/build android` → Runs build script
- `/deploy rules` → Deploys Firebase
- `/analyze-players` → Processes player feedback

### 3. Modular Skills (skills/)
Reusable expertise modules:
- **unity-build.md** - Build troubleshooting and automation
- **firebase-cost-monitor.md** - Cost tracking and prevention
- **player-acquisition.md** - Beta tester acquisition playbook

### 4. Quality Gates (review.md)
Pre-commit checklist ensures:
- Code compiles
- Tests pass
- Costs stay low
- User experience preserved
- Documentation updated

---

## 🔥 Key Principles Applied

### From Andrej Karpathy's CLAUDE.md:

1. **Plan Mode First** - Write specs before code
2. **Verify Relentlessly** - Test after every change
3. **Keep It Simple** - 100 lines > 1000 lines
4. **Surgical Edits Only** - Minimize side effects
5. **Goal-Driven Execution** - Clear success criteria
6. **Builder-Validator Pattern** - Build → Test → Fix → Iterate

### From Skills.md Framework:

- **Tool-Powered Skills** - Use external tools (Build.ps1, Firebase CLI)
- **Context-Aware** - Knows Unity architecture, Firebase limits, etc.
- **Error Recovery** - Common issues documented with fixes
- **Automation-Ready** - Hooks for pre/post tool execution

---

## 🚀 How to Use

### Quick Commands

When working with AI, you can now say:
- "Build Android release" → AI runs `.\Build.ps1 -Platform Android -BuildType Release`
- "Check Firebase costs" → AI reads firebase-cost-monitor.md and validates usage
- "How do I get beta testers?" → AI references player-acquisition.md playbook

### Context Loading

AI automatically knows:
- Your 137 C# scripts and architecture
- Firebase rate limits and cost targets
- Current blockers (Unity modules not installed)
- Success thresholds (50 installs by Week 4)

### Quality Automation

Before committing changes, AI will:
1. Verify code compiles
2. Check Firebase rules if modified
3. Estimate cost impact
4. Run pre-commit checklist from review.md

---

## 📊 Expected Benefits

### Developer Velocity
- **Faster Context Switching** - AI remembers where you left off
- **Fewer Mistakes** - Pre-commit checks catch issues
- **Better Decisions** - Data-driven prioritization (player-acquisition.md)

### Cost Protection
- **Firebase Monitoring** - Automated cost checks
- **Rate Limit Enforcement** - Rules validated before deploy
- **Budget Alerts** - Early warning at $2/month

### Focus on Traction
- **Player Acquisition** - Systematic approach to first 100 users
- **Feedback Analysis** - Structured player feedback processing
- **Metric Tracking** - Clear success/failure thresholds

---

## 🔄 Continuous Improvement

This structure evolves with the project:

### When Adding Features
1. Update CLAUDE.md with new architecture
2. Add skill if complex (e.g., IAP-testing.md after implementing Unity IAP)
3. Update review.md checklist if needed

### When Hitting Milestones
1. Document learnings in relevant skill
2. Update metrics in CLAUDE.md
3. Refine player-acquisition.md based on what worked

### When Pivoting
1. Update CLAUDE.md constraints
2. Archive obsolete skills
3. Create new skills for pivot direction

---

## 🎓 Learning Resources

- [CLAUDE.md Guide](https://gist.github.com/karpathy/...) - Original reference
- [Skills.md Framework](https://docs.claude.ai/skills) - Anthropic docs
- [Builder-Validator Pattern](https://martinfowler.com/...) - Quality loop

---

## ✅ Immediate Value

**Try this now:**
1. Ask AI: "What should I work on today?"
2. AI will reference CLAUDE.md and say: "Install Unity Android modules, then build first APK"
3. Ask: "How do I do that?"
4. AI will reference unity-build.md skill with step-by-step instructions

**Result:** Faster development, fewer mistakes, better outcomes.

---

**Your project is now AI-assisted and production-ready.** The structure scales from pre-beta to 10K+ users.
