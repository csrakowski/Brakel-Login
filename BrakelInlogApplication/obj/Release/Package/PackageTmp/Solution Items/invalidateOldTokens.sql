﻿DELETE FROM [token] WHERE DATEDIFF(minute, [createDateTime], cast(getDate() as date)) >= 20