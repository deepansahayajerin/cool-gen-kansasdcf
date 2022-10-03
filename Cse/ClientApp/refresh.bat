pushd %~dp0..\..\..\bphx.cool.angular
call npm install
call npm run build
cd dist\bphx-cool
call npm link
popd

call npm link @adv-appmod/bphx-cool
rmdir %~dp0.angular /s /q
