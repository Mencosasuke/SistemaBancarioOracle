----------------------------------CREACIÓN DE USUARIOS Y ASIGNACIÓN DE PERMISOS----------------------------------
CREATE USER ADMINBANC
IDENTIFIED BY adminbanc
DEFAULT TABLESPACE users;
  
GRANT CREATE SESSION TO ADMINBANC;

GRANT SELECT, INSERT, UPDATE, DELETE ON SYSBANC.CUENTA TO ADMINBANC;
GRANT SELECT, INSERT, UPDATE, DELETE ON SYSBANC.BITACORA TO ADMINBANC;

GRANT EXECUTE ON SYSBANC.pkg_account_management to ADMINBANC;



----------------------------------TRIGGER PARA EL MANEJO DE LA BITÁCORA----------------------------------

CREATE OR REPLACE TRIGGER TRIG_CUENTA
  AFTER INSERT OR UPDATE OR DELETE
  ON CUENTA
  REFERENCING NEW AS NEW OLD AS OLD
  FOR EACH ROW
DECLARE
  vl_id BITACORA.ID%TYPE;
BEGIN
  SELECT (NVL(MAX(ID), 0)+1) INTO vl_id FROM BITACORA;

  IF INSERTING THEN
    INSERT INTO BITACORA
    VALUES(vl_id, USER, SYSDATE, 'INSERT', 'SATISFACTORIO', 'LINEA INSERTADA: '||:NEW.NOMBRE ||'-'
           ||:NEW.APELLIDO ||'-'||:NEW.SALDO ||'-'||:NEW.INTERES ||'-'||:NEW.STATUS ||'-'||:NEW.CUENTA);
  
  ELSIF UPDATING THEN
    INSERT INTO BITACORA
    VALUES(vl_id, USER, SYSDATE, 'UPDATE', 'SATISFACTORIO', 'LINEA ANTIGUA '||:OLD.NOMBRE ||'-'||:OLD.APELLIDO
           ||'-'||:OLD.SALDO ||'-'||:OLD.INTERES ||'-'||:OLD.STATUS ||'-'||:OLD.CUENTA ||' LINEA NUEVA '||:NEW.NOMBRE
           ||'-'||:NEW.APELLIDO ||'-'||:NEW.SALDO ||'-'||:NEW.INTERES ||'-'||:NEW.STATUS ||'-'||:NEW.CUENTA);
    
  ELSIF DELETING THEN
      INSERT INTO BITACORA
      VALUES(vl_id, USER, SYSDATE, 'DELETE', 'SATISFACTORIO', 'CUENTA ELIMINADA: '||:OLD.CUENTA);
  END IF;
END;

----------------------------------FUNCIONES GENERALES DEL SISTEMA----------------------------------

CREATE OR REPLACE PACKAGE pkg_account_management AS
  FUNCTION insert_account(vl_cuenta CUENTA.CUENTA%TYPE, vl_nombre CUENTA.NOMBRE%TYPE, vl_apellido CUENTA.APELLIDO%TYPE, vl_saldo CUENTA.SALDO%TYPE, vl_interes CUENTA.INTERES%TYPE) RETURN NVARCHAR2;
  FUNCTION update_account(vl_cuenta CUENTA.CUENTA%TYPE, vl_nombre CUENTA.NOMBRE%TYPE, vl_apellido CUENTA.APELLIDO%TYPE, vl_saldo CUENTA.SALDO%TYPE, vl_interes CUENTA.INTERES%TYPE) RETURN NVARCHAR2;
  FUNCTION delete_account(vl_cuenta CUENTA.CUENTA%TYPE) RETURN NVARCHAR2;
  FUNCTION reopen_account(vl_cuenta CUENTA.CUENTA%TYPE) RETURN NVARCHAR2;
  
  FUNCTION transf_saldos(vl_cuentaOrigen CUENTA.CUENTA%TYPE, vl_cuentaDestino CUENTA.CUENTA%TYPE, vl_monto CUENTA.SALDO%TYPE) RETURN NVARCHAR2;
  FUNCTION aumentar_saldos(vl_monto CUENTA.SALDO%TYPE) RETURN NVARCHAR2;
  FUNCTION decrementar_saldos(vl_monto CUENTA.SALDO%TYPE) RETURN NVARCHAR2;
  FUNCTION abono_cuenta(vl_cuenta CUENTA.CUENTA%TYPE, vl_monto CUENTA.SALDO%TYPE) RETURN NVARCHAR2;
  FUNCTION retiro_cuenta(vl_cuenta CUENTA.CUENTA%TYPE, vl_monto CUENTA.SALDO%TYPE) RETURN NVARCHAR2;
  
  FUNCTION calcular_intereses RETURN NVARCHAR2;
  FUNCTION calcular_interes(vl_cuenta CUENTA.CUENTA%TYPE)RETURN NVARCHAR2;
  
  PROCEDURE guardar_bitacora(vl_accion BITACORA.ACCION%TYPE, vl_descError BITACORA.DESC_ERROR%TYPE);
END;

CREATE OR REPLACE PACKAGE BODY pkg_account_management AS
  
  FUNCTION insert_account(
    vl_cuenta CUENTA.CUENTA%TYPE,
    vl_nombre CUENTA.NOMBRE%TYPE,
    vl_apellido CUENTA.APELLIDO%TYPE,
    vl_saldo CUENTA.SALDO%TYPE,
    vl_interes CUENTA.INTERES%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    vl_cuentaAux NUMBER;
  BEGIN
    SELECT COUNT(*)
    INTO vl_cuentaAux
    FROM CUENTA
    WHERE CUENTA = vl_cuenta;
    
    IF vl_cuentaAux > 0 THEN
      guardar_bitacora('INSERT', 'El numero de cuenta que desea ingresar ya existe.');
      RETURN '3||El numero de cuenta que desea ingresar ya existe.';
    END IF;
    
    INSERT INTO CUENTA(CUENTA, NOMBRE, APELLIDO, SALDO, INTERES, STATUS)
    VALUES(vl_cuenta, vl_nombre, vl_apellido, vl_saldo, vl_interes, 'X');
    COMMIT;
    RETURN '1|Cuenta ingresada satisfactoriamente.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('INSERT', 'Error al insertar nueva cuenta ' || vl_cuenta || '.');
      ROLLBACK;
      RETURN '2|Error al insertar nueva cuenta ' || vl_cuenta || '.';
  END;  
  
  FUNCTION update_account(
    vl_cuenta CUENTA.CUENTA%TYPE,
    vl_nombre CUENTA.NOMBRE%TYPE,
    vl_apellido CUENTA.APELLIDO%TYPE,
    vl_saldo CUENTA.SALDO%TYPE,
    vl_interes CUENTA.INTERES%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE CUENTA
    SET NOMBRE = vl_nombre,
        APELLIDO = vl_apellido,
        SALDO = vl_saldo,
        INTERES = vl_interes
    WHERE CUENTA = vl_cuenta;
    COMMIT;
    RETURN '1|Cuenta actualizada satisfatoriamente.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('UPDATE', 'Error al actualizar datos de la cuenta ' || vl_cuenta || '.');
      ROLLBACK;
      RETURN '2|Error al actualizar datos de la cuenta ' || vl_cuenta || '.';
  END;
  
  FUNCTION delete_account(
    vl_cuenta CUENTA.CUENTA%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE CUENTA
    SET STATUS = ''
    WHERE CUENTA = vl_cuenta;
    COMMIT;
    RETURN '1|Cuenta ' || vl_cuenta || ' cerrada satisfatoriamente.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('DELETE', 'Error al intentar dar de baja a la cuenta ' || vl_cuenta || '.');
      ROLLBACK;
      RETURN '2|Error al intentar dar de baja a la cuenta ' || vl_cuenta || '.';
  END;
  
  FUNCTION reopen_account(
    vl_cuenta CUENTA.CUENTA%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE CUENTA
    SET STATUS = 'X'
    WHERE CUENTA = vl_cuenta;
    COMMIT;
    RETURN '1|Cuenta ' || vl_cuenta || ' reabierta satisfatoriamente.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('DELETE', 'Error al intentar dar de alta a la cuenta ' || vl_cuenta || '.');
      ROLLBACK;
      RETURN '2|Error al intentar dar de alta a la cuenta ' || vl_cuenta || '.';
  END;
  
  FUNCTION transf_saldos(
    vl_cuentaOrigen CUENTA.CUENTA%TYPE,
    vl_cuentaDestino CUENTA.CUENTA%TYPE,
    vl_monto CUENTA.SALDO%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    vl_saldoOrigen CUENTA.SALDO%TYPE;
    vl_destinoAux NUMBER;
  BEGIN
    SELECT SALDO
    INTO vl_saldoOrigen
    FROM CUENTA
    WHERE CUENTA = vl_cuentaOrigen;
    
    IF vl_monto > vl_saldoOrigen THEN
      guardar_bitacora('UPDATE', 'El saldo de la cuenta ' || vl_cuentaOrigen || ' es insuficiente para realizar la transacción.');
      RETURN '3|El saldo de la cuenta ' || vl_cuentaOrigen || ' es insuficiente para realizar la transacción.';
    END IF;
    
    SELECT COUNT(*)
    INTO vl_destinoAux
    FROM CUENTA
    WHERE CUENTA = vl_cuentaDestino AND STATUS = 'X';
    
    IF vl_destinoAux <= 0 THEN
      guardar_bitacora('UPDATE', 'La cuenta de destino ' || vl_cuentaDestino || ' esta inactiva o no existe.');
      RETURN '3|La cuenta de destino ' || vl_cuentaDestino || ' esta inactiva o no existe.';
    END IF;
    
    UPDATE CUENTA
    SET SALDO = CUENTA.SALDO - vl_monto
    WHERE CUENTA = vl_cuentaOrigen;
    
    UPDATE CUENTA
    SET SALDO = CUENTA.SALDO + vl_monto
    WHERE CUENTA = vl_cuentaDestino;
    
    COMMIT;
    RETURN '1|Monto de ' || vl_monto || ' transferido satisfatoriamente de la cuenta ' || vl_cuentaOrigen || ' a la cuenta ' || vl_cuentaDestino || '.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('UPDATE', 'Error al intentar hacer la transferencia de la cuenta ' || vl_cuentaOrigen || ' a la cuenta ' || vl_cuentaDestino || '.');
      ROLLBACK;
      RETURN '2|Error al intentar hacer la transferencia de la cuenta ' || vl_cuentaOrigen || ' a la cuenta ' || vl_cuentaDestino || '.';
  END;
  
  FUNCTION aumentar_saldos(
    vl_monto CUENTA.SALDO%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE CUENTA
    SET SALDO = CUENTA.SALDO + vl_monto
    WHERE STATUS = 'X';
    COMMIT;
    RETURN '1|El monto de ' || vl_monto || ' fue correctamente abonado a todas las cuentas del sistema.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('UPDATE', 'Error al intentar aumentar el saldo de las cuentas del sistema.');
      ROLLBACK;
      RETURN '2|Error al intentar aumentar el saldo de las cuentas del sistema.';
  END;    
  
  FUNCTION decrementar_saldos(
    vl_monto CUENTA.SALDO%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    CURSOR vl_cursorCuentas IS SELECT CUENTA, SALDO FROM CUENTA WHERE STATUS = 'X';
  BEGIN
    FOR c_cuenta in vl_cursorCuentas LOOP
      IF vl_monto > c_cuenta.SALDO THEN
        ROLLBACK;
        guardar_bitacora('UPDATE', 'El saldo actual de la cuenta ' || c_cuenta.CUENTA ||' es insuficiente para realizar el decremento en las cuentas del sistema.');
        RETURN '3|El saldo actual de la cuenta ' || c_cuenta.CUENTA ||' es insuficiente para realizar el decremento en las cuentas del sistema.';
      ELSE
        UPDATE CUENTA
        SET SALDO = CUENTA.SALDO - vl_monto
        WHERE CUENTA = c_cuenta.CUENTA;
      END IF;
    END LOOP;
    
    COMMIT;
    RETURN '1|El monto de ' || vl_monto || ' fue correctamente retirado de todas las cuentas del sistema.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('UPDATE', 'Error al intentar decrementar el saldo de las cuentas del sistema.');
      ROLLBACK;
      RETURN '2|Error al intentar decrementar el saldo de las cuentas del sistema.';
  END;  

  FUNCTION abono_cuenta(
    vl_cuenta CUENTA.CUENTA%TYPE,
    vl_monto CUENTA.SALDO%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE CUENTA
    SET SALDO = CUENTA.SALDO + vl_monto
    WHERE CUENTA = vl_cuenta;
    COMMIT;
    RETURN '1|El monto de ' || vl_monto || ' fue correctamente abonado a la cuenta ' || vl_cuenta || '.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('UPDATE', 'Error al intentar abonar la cuenta ' || vl_cuenta || '.');
      ROLLBACK;
      RETURN '2|Error al intentar abonar la cuenta ' || vl_cuenta || '.';
  END; 
  
  FUNCTION retiro_cuenta(
    vl_cuenta CUENTA.CUENTA%TYPE,
    vl_monto CUENTA.SALDO%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    vl_saldoActual CUENTA.SALDO%TYPE;
  BEGIN
    SELECT SALDO
    INTO vl_saldoActual
    FROM CUENTA
    WHERE CUENTA = vl_cuenta;
    
    IF vl_monto > vl_saldoActual THEN
      guardar_bitacora('UPDATE', 'El saldo actual de la cuenta ' || vl_cuenta || ' es insuficiente para realizar la transacción.');
      RETURN '3|El saldo actual de la cuenta ' || vl_cuenta || ' es insuficiente para realizar la transacción.';
    END IF;
    
    UPDATE CUENTA
    SET SALDO = CUENTA.SALDO - vl_monto
    WHERE CUENTA = vl_cuenta;
    COMMIT;
    RETURN '1|El monto de ' || vl_monto || ' fue correctamente retirado de la cuenta ' || vl_cuenta || '.';
  EXCEPTION
    WHEN OTHERS THEN
      guardar_bitacora('UPDATE', 'Error al intentar hacer el retiro de la cuenta ' || vl_cuenta || '.');
      ROLLBACK;
      RETURN '2|Error al intentar hacer el retiro de la cuenta ' || vl_cuenta || '.';
  END;
  
  FUNCTION calcular_intereses
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    CURSOR vl_cursorCuentas IS SELECT CUENTA, SALDO, NVL(INTERES, 0) INTERES FROM CUENTA WHERE STATUS = 'X';
  BEGIN
    FOR v_field IN vl_cursorCuentas LOOP
      UPDATE CUENTA
      SET SALDO = ( v_field.SALDO * v_field.INTERES ) + v_field.SALDO
      WHERE CUENTA = v_field.CUENTA;
    END LOOP;
    COMMIT;
    RETURN '1|Los intereses de las cuentas del sistema fueron correctamente calculados.';
  EXCEPTION
    WHEN OTHERS THEN
    guardar_bitacora('UPDATE', 'Error al calcular los intereses de las cuentas del sistema.');
    ROLLBACK;
    RETURN '2|Error al calcular los intereses de las cuentas del sistema.';
  END;
  
  FUNCTION calcular_interes(
    vl_cuenta CUENTA.CUENTA%TYPE
  )
  RETURN NVARCHAR2
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE CUENTA
    SET SALDO = ( CUENTA.SALDO * CUENTA.INTERES ) + CUENTA.SALDO
    WHERE CUENTA = vl_cuenta;
    COMMIT;
    RETURN '1|Los intereses de la cuenta ' || vl_cuenta || ' fueron correctamente calculados.';
  EXCEPTION
    WHEN OTHERS THEN
    guardar_bitacora('UPDATE', 'Error al calcular los intereses de la cuenta ' || vl_cuenta || '.');
    ROLLBACK;
    RETURN '2|Error al calcular los intereses de la cuenta ' || vl_cuenta || '.';
  END;
  
  PROCEDURE guardar_bitacora(
    vl_accion BITACORA.ACCION%TYPE,
    vl_descError BITACORA.DESC_ERROR%TYPE
  )
  IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    vl_id BITACORA.ID%TYPE;
  BEGIN
    SELECT (NVL(MAX(ID), 0)+1) INTO vl_id FROM BITACORA;
    INSERT INTO BITACORA(ID, ACCION, DESC_ERROR)
    VALUES (vl_id, vl_accion, vl_descError);
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
  END;
END;