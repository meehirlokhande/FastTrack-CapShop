import { useEffect } from "react";
import { useAuthStore } from "./app/authStore";
import AppRoutes from "./routes/AppRoutes";

export default function App() {
  const hydrate = useAuthStore((s) => s.hydrate);

  useEffect(() => {
    hydrate();
  }, [hydrate]);

  return <AppRoutes />;
}
