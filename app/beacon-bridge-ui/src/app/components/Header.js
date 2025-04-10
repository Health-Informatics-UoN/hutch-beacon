import Image from "next/image";

export default function Header() {
  return (
    <div className="bg-uon-gradient">
      <nav
        className="mx-4 flex max-w-7xl items-center justify-between p-6 lg:px-1"
        aria-label="Global"
      >
        <span className={"flex items-baseline space-x-5"}>
          <Image
            src={"/UoN-Nottingham-Blue-white-text-logo-RGB-300x112.png"}
            width={200}
            height={112}
          />
          <h1 className={"text-6xl text-white"}>Beacon</h1>
        </span>
      </nav>
    </div>
  );
}
