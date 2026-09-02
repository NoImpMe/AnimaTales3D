# FAIL.md — 과거 실패 목록

같은 실수를 반복하지 않기 위한 기록. 형식: `- [날짜] 증상 → 원인 → 방지 규칙`

- [2026-09-02] .gitignore를 프로젝트 루트(`AnimaTales2D/`) 기준으로 고쳤는데도 이미 스테이징돼 있던 대량 파일(Library/Temp/Obj 등)이 계속 스테이징 상태로 남음 → gitignore는 "새로 add되는 파일"만 막고, 이미 인덱스에 올라간 파일은 자동으로 안 빠짐 → 프로젝트 폴더 구조를 옮기거나(예: `My project/` 중간 폴더 제거) gitignore 기준 경로를 바꾼 뒤에는 `git rm -r --cached <경로>`로 기존 인덱스에서 명시적으로 제거하고, `git status --short`로 재확인할 것
