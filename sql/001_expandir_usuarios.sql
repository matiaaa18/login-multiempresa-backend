BEGIN;

CREATE TABLE IF NOT EXISTS sexos (
    id INT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO sexos (id, nombre)
VALUES
    (1, 'Masculino'),
    (2, 'Femenino'),
    (3, 'Otros')
ON CONFLICT (id) DO NOTHING;

ALTER TABLE usuarios
    ADD COLUMN IF NOT EXISTS nombre VARCHAR(100),
    ADD COLUMN IF NOT EXISTS apellido_paterno VARCHAR(100),
    ADD COLUMN IF NOT EXISTS apellido_materno VARCHAR(100),
    ADD COLUMN IF NOT EXISTS fecha_nacimiento TIMESTAMPTZ NULL,
    ADD COLUMN IF NOT EXISTS direccion VARCHAR(100),
    ADD COLUMN IF NOT EXISTS id_sexo INT,
    ADD COLUMN IF NOT EXISTS correo VARCHAR(100),
    ADD COLUMN IF NOT EXISTS fecha_ingreso TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS vigencia BOOLEAN,
    ADD COLUMN IF NOT EXISTS url_imagen VARCHAR(200),
    ADD COLUMN IF NOT EXISTS renovar_pwd BOOLEAN;

UPDATE usuarios u
SET nombre = COALESCE(u.nombre, INITCAP(u.nombre_usuario)),
    apellido_paterno = COALESCE(u.apellido_paterno, 'Pendiente'),
    apellido_materno = COALESCE(u.apellido_materno, NULL),
    direccion = COALESCE(u.direccion, NULL),
    id_sexo = COALESCE(u.id_sexo, 3),
    correo = COALESCE(
        u.correo,
        u.nombre_usuario || '+' || e.subdominio || '@demo.local'
    ),
    fecha_ingreso = COALESCE(u.fecha_ingreso, u.creado_en),
    vigencia = COALESCE(u.vigencia, u.activo),
    url_imagen = COALESCE(u.url_imagen, NULL),
    renovar_pwd = COALESCE(u.renovar_pwd, FALSE)
FROM empresas e
WHERE u.id_empresa = e.id;

ALTER TABLE usuarios
    ALTER COLUMN nombre SET NOT NULL,
    ALTER COLUMN apellido_paterno SET NOT NULL,
    ALTER COLUMN id_sexo SET NOT NULL,
    ALTER COLUMN correo SET NOT NULL,
    ALTER COLUMN fecha_ingreso SET NOT NULL,
    ALTER COLUMN vigencia SET NOT NULL,
    ALTER COLUMN renovar_pwd SET NOT NULL;

ALTER TABLE usuarios
    ALTER COLUMN fecha_ingreso SET DEFAULT CURRENT_TIMESTAMP,
    ALTER COLUMN vigencia SET DEFAULT TRUE,
    ALTER COLUMN renovar_pwd SET DEFAULT FALSE;

ALTER TABLE usuarios
    DROP CONSTRAINT IF EXISTS fk_usuarios_sexos;

ALTER TABLE usuarios
    ADD CONSTRAINT fk_usuarios_sexos
    FOREIGN KEY (id_sexo) REFERENCES sexos(id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'uq_usuarios_correo'
    ) THEN
        ALTER TABLE usuarios
            ADD CONSTRAINT uq_usuarios_correo UNIQUE (correo);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_usuarios_busqueda
ON usuarios (nombre, apellido_paterno, correo);

COMMIT;
