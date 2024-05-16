import { Inter } from "next/font/google";
import "./globals.css";
import Header from "@/app/components/Header";

const inter = Inter({ subsets: ["latin"] });

export const metadata = {
  title: "Beacon",
  description: "Beacon by University of Nottingham",
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
