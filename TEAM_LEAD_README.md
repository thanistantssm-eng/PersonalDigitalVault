# Team Lead Base

This is the **initial shared repository base only**. Do NOT push the original full project ZIP to `main` first, otherwise the developers will have nothing meaningful to add from their feature branches.

## Team leader commands

```bash
git init
git branch -M main
git add .
git commit -m "chore: initialize Personal Digital Vault team repository"
git remote add origin YOUR_REPOSITORY_URL
git push -u origin main

git switch -c develop
git push -u origin develop
```

Copy the contents of `REPO_BASE` into the repository root before the first commit.

After that, all 4 developers branch from `develop`. Merge feature branches into `develop`; only after integration testing merge `develop` into `main`.
