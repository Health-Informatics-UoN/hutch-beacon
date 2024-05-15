import { inter } from "@/ui/fonts";
import "./globals.css";
import Header from "@/app/components/Header";

export const metadata = {
  title: "BEACON",
  description: "BEACON by University of Nottingham",
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Header />
        {children}
        </body>
    </html>
  );
}
