@ECHO OFF

echo.

if "%JRE_HOME%" == "" goto :use_jdk
	set _JAVA_HOME="%JRE_HOME%"
	echo Using  JRE_HOME : %_JAVA_HOME%
	goto :set_classpath
	
:use_jdk
set _JAVA_HOME="%JAVA_HOME%"
echo Using JAVA_HOME : %_JAVA_HOME%

:set_classpath
set _CLASSPATH="%CLASSPATH%"
echo Using CLASSPATH : %_CLASSPATH%

set _JAVA=%_JAVA_HOME%\bin\javaw

set APP_MAIN_CLASS=pfm.ClientApp
set TITLE="PFM-Client"
set APP_PATH="%~dp0."
set APP_CLASSPATH=%APP_PATH%\classes
set APP_LIBPATH=%APP_PATH%\lib
set JVM_OPS=-server

@ECHO ON

start %TITLE% /B %_JAVA% %JVM_OPS% -Duser.dir=%APP_PATH% -Djava.ext.dirs=%APP_LIBPATH% -cp %APP_CLASSPATH%;%_CLASSPATH% %APP_MAIN_CLASS% %*
